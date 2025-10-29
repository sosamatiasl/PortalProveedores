-- =====================================================================
-- Script de Creación de Base de Datos: PortalProveedoresDB
-- Servidor: localhost
-- Base de Datos: PortalProveedoresDB
-- =====================================================================

-- 1. CREACIÓN DE LA BASE DE DATOS
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PortalProveedoresDB')
BEGIN
    CREATE DATABASE PortalProveedoresDB;
END
GO

USE PortalProveedoresDB;
GO

-- =====================================================================
-- 2. TABLAS DE SEGURIDAD Y USUARIOS
-- =====================================================================

-- Roles definidos en el sistema (A, B, C, D, E)
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL UNIQUE, -- Ej: "AdminCliente", "AdminProveedor", "Transportista"
    Descripcion NVARCHAR(255) NULL
);
GO

-- Tabla de Clientes (La empresa cliente)
CREATE TABLE Clientes (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    RazonSocial NVARCHAR(255) NOT NULL,
    CUIT NVARCHAR(11) NOT NULL UNIQUE,
    FechaCreacion DATETIME2(7) NOT NULL DEFAULT GETDATE()
);
GO

-- Tabla de Proveedores (La empresa proveedora)
CREATE TABLE Proveedores (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    RazonSocial NVARCHAR(255) NOT NULL,
    CUIT NVARCHAR(11) NOT NULL UNIQUE,
    FechaCreacion DATETIME2(7) NOT NULL DEFAULT GETDATE()
);
GO

-- Tabla central de Usuarios
CREATE TABLE Usuarios (
    Id NVARCHAR(450) PRIMARY KEY, -- Usamos NVARCHAR para compatibilidad con Identity (Google, Microsoft)
    Email NVARCHAR(250) NOT NULL UNIQUE,
    NombreCompleto NVARCHAR(250) NOT NULL,
    SelfieFotoURL NVARCHAR(1024) NULL, -- Path al blob storage con la foto selfie
    ProveedorId BIGINT NULL, -- NULL si es usuario Cliente
    ClienteId BIGINT NULL,   -- NULL si es usuario Proveedor
    FechaRegistro DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1,

    -- Un usuario pertenece a UN cliente O a UN proveedor
    CONSTRAINT FK_Usuario_Proveedor FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id),
    CONSTRAINT FK_Usuario_Cliente FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    CONSTRAINT CHK_Usuario_ClienteProveedor CHECK (
        (ProveedorId IS NOT NULL AND ClienteId IS NULL) OR 
        (ProveedorId IS NULL AND ClienteId IS NOT NULL)
    )
);
GO

-- Tabla de relación N-N entre Usuarios y Roles
CREATE TABLE UsuarioRoles (
    UsuarioId NVARCHAR(450) NOT NULL,
    RolId INT NOT NULL,
    PRIMARY KEY (UsuarioId, RolId),
    CONSTRAINT FK_UsuarioRoles_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioRoles_Rol FOREIGN KEY (RolId) REFERENCES Roles(Id) ON DELETE CASCADE
);
GO

-- Pre-cargamos los roles definidos
INSERT INTO Roles (Nombre, Descripcion) VALUES
('AdministrativoCliente', 'Rol A: Administrativo de parte del Cliente.'),
('AdministrativoProveedor', 'Rol B: Administrativo de parte del Proveedor.'),
('Transportista', 'Rol C: Transportista de mercadería.'),
('RecepcionadorMercaderia', 'Rol D: Recepcionador de mercadería en depósito de cliente.'),
('DespachanteMercaderia', 'Rol E: Despachante de mercadería desde depósito de proveedor.');
GO

-- =====================================================================
-- 3. TABLAS DEL CIRCUITO DE COMPRA (A, B, C)
-- =====================================================================

-- A) El cliente emite una orden de compra
CREATE TABLE OrdenesCompra (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    ClienteId BIGINT NOT NULL,
    ProveedorId BIGINT NOT NULL,
    NumeroOrden NVARCHAR(100) NOT NULL,
    FechaEmision DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    Estado INT NOT NULL, -- Ej: 0: Pendiente, 1: Cotizada, 2: Cerrada
    Detalles TEXT NULL,

    CONSTRAINT FK_OrdenCompra_Cliente FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    CONSTRAINT FK_OrdenCompra_Proveedor FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id)
);
GO
CREATE INDEX IX_OrdenesCompra_ClienteId ON OrdenesCompra(ClienteId);
CREATE INDEX IX_OrdenesCompra_ProveedorId ON OrdenesCompra(ProveedorId);
GO

-- B) El proveedor envia una cotizacion
CREATE TABLE Cotizaciones (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    OrdenCompraId BIGINT NOT NULL,
    ProveedorId BIGINT NOT NULL,
    NumeroCotizacion NVARCHAR(100) NOT NULL,
    FechaEmision DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    MontoTotal DECIMAL(18, 2) NOT NULL,
    ValidezDias INT NOT NULL DEFAULT 15,
    Estado INT NOT NULL, -- Ej: 0: Enviada, 1: Aceptada, 2: Rechazada
    ArchivoPDF_URL NVARCHAR(1024) NULL,

    CONSTRAINT FK_Cotizacion_OrdenCompra FOREIGN KEY (OrdenCompraId) REFERENCES OrdenesCompra(Id),
    CONSTRAINT FK_Cotizacion_Proveedor FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id)
);
GO
CREATE INDEX IX_Cotizaciones_OrdenCompraId ON Cotizaciones(OrdenCompraId);
GO

-- =====================================================================
-- 4. TABLAS DEL CIRCUITO DE LOGÍSTICA (D, E)
-- =====================================================================

-- D) El proveedor envia la mercaderia y acompaña un remito
CREATE TABLE Remitos (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    ProveedorId BIGINT NOT NULL,
    ClienteId BIGINT NOT NULL,
    NumeroRemito NVARCHAR(100) NOT NULL,
    FechaEmision DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    Estado INT NOT NULL, -- Ej: 0: PendienteEnvio, 1: EnTransporte, 2: Recibido, 3: RecibidoConDiferencias
    ArchivoPDF_URL NVARCHAR(1024) NULL, -- Copia escaneada del remito

    CONSTRAINT FK_Remito_Proveedor FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id),
    CONSTRAINT FK_Remito_Cliente FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);
GO

-- Relación N-N: Un remito puede estar asociado a varias cotizaciones (aceptadas)
CREATE TABLE CotizacionRemitos (
    CotizacionId BIGINT NOT NULL,
    RemitoId BIGINT NOT NULL,
    PRIMARY KEY (CotizacionId, RemitoId),
    CONSTRAINT FK_CotRem_Cotizacion FOREIGN KEY (CotizacionId) REFERENCES Cotizaciones(Id),
    CONSTRAINT FK_CotRem_Remito FOREIGN KEY (RemitoId) REFERENCES Remitos(Id)
);
GO

-- E) Recepcion de mercaderia en deposito
CREATE TABLE Recepciones (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    RemitoId BIGINT NOT NULL UNIQUE, -- Una unica recepcion por remito
    UsuarioRecepcionId NVARCHAR(450) NOT NULL, -- Usuario con Rol "D"
    FechaRecepcion DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    HuboDiferencias BIT NOT NULL DEFAULT 0,
    DetalleDiferencias TEXT NULL,
    FirmaRecepcionista_URL NVARCHAR(1024) NOT NULL, -- Path a la imagen de la firma
    FirmaTransportista_URL NVARCHAR(1024) NOT NULL, -- Path a la imagen de la firma

    CONSTRAINT FK_Recepcion_Remito FOREIGN KEY (RemitoId) REFERENCES Remitos(Id),
    CONSTRAINT FK_Recepcion_Usuario FOREIGN KEY (UsuarioRecepcionId) REFERENCES Usuarios(Id)
);
GO

-- Detalle de las cantidades recibidas (si hubo diferencias)
CREATE TABLE RecepcionDetalles (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    RecepcionId BIGINT NOT NULL,
    IdProducto NVARCHAR(100) NOT NULL, -- SKU o ID de producto
    DescripcionProducto NVARCHAR(255) NOT NULL,
    CantidadDeclarada DECIMAL(18, 2) NOT NULL, -- Lo que decia el remito
    CantidadRecibida DECIMAL(18, 2) NOT NULL,  -- Lo que se contó

    CONSTRAINT FK_RecepcionDetalle_Recepcion FOREIGN KEY (RecepcionId) REFERENCES Recepciones(Id)
);
GO

-- QR para el transporte (Punto 2 de Aclaraciones)
CREATE TABLE RemitoQRCodes (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    RemitoId BIGINT NOT NULL UNIQUE,
    CodigoHash NVARCHAR(1024) NOT NULL, -- El dato encriptado y hasheado
    FechaExpiracion DATETIME2(7) NOT NULL,
    Usado BIT NOT NULL DEFAULT 0, -- Se marca como usado al leerlo en destino

    CONSTRAINT FK_RemitoQR_Remito FOREIGN KEY (RemitoId) REFERENCES Remitos(Id)
);
GO

-- =====================================================================
-- 5. TABLAS DEL CIRCUITO DE FACTURACIÓN (F)
-- =====================================================================

-- F) El proveedor adjunta la factura
CREATE TABLE Facturas (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    ProveedorId BIGINT NOT NULL,
    ArchivoPDF_URL NVARCHAR(1024) NOT NULL,
    FechaCarga DATETIME2(7) NOT NULL DEFAULT GETDATE(),

    -- Estado del procesamiento IA (F.1 a F.11)
    EstadoProcesamiento INT NOT NULL DEFAULT 0, -- 0: Pendiente, 1: Procesada, 2: Error
    
    -- Datos extraídos por la IA (F.1 a F.11)
    CuitEmisor NVARCHAR(11) NULL,
    RazonSocialEmisor NVARCHAR(255) NULL,
    FechaEmision DATETIME2(7) NULL,
    TipoFactura INT NULL, -- 1: Factura, 2: Nota de Crédito, 3: Nota de Débito
    LetraFactura CHAR(1) NULL, -- 'A', 'B', 'C'
    CuitDestinatario NVARCHAR(11) NULL,
    CAE NVARCHAR(100) NULL,
    TotalSinImpuestos DECIMAL(18, 2) NULL,
    TotalImpuestos DECIMAL(18, 2) NULL,
    TotalConImpuestos DECIMAL(18, 2) NULL,
    
    -- Validación AFIP (F.6)
    EstadoValidacionAFIP INT NOT NULL DEFAULT 0, -- 0: Pendiente, 1: Válido, 2: Inválido (Rechazado)
    MensajeValidacionAFIP NVARCHAR(500) NULL,

    -- Verificación de consistencia (F.5)
    ConsistenciaCUIT BIT NULL, -- 1 si CuitDestinatario == CuitCliente

    CONSTRAINT FK_Factura_Proveedor FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id)
);
GO

-- Relación N-N: Una factura puede estar linkeada a varios remitos (recepcionados)
CREATE TABLE FacturaRemitos (
    FacturaId BIGINT NOT NULL,
    RemitoId BIGINT NOT NULL,
    PRIMARY KEY (FacturaId, RemitoId),
    CONSTRAINT FK_FacRem_Factura FOREIGN KEY (FacturaId) REFERENCES Facturas(Id),
    CONSTRAINT FK_FacRem_Remito FOREIGN KEY (RemitoId) REFERENCES Remitos(Id)
);
GO

-- Detalle de items de la factura (extraídos por IA) (F.7, F.8)
CREATE TABLE FacturaDetalles (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    FacturaId BIGINT NOT NULL,
    IdProducto NVARCHAR(100) NULL, -- SKU o ID de producto
    Descripcion NVARCHAR(500) NOT NULL,
    Cantidad DECIMAL(18, 2) NOT NULL,
    PrecioUnitario DECIMAL(18, 2) NOT NULL,
    
    CONSTRAINT FK_FacturaDetalle_Factura FOREIGN KEY (FacturaId) REFERENCES Facturas(Id)
);
GO

PRINT 'Base de datos PortalProveedoresDB creada y estructurada exitosamente.';
GO