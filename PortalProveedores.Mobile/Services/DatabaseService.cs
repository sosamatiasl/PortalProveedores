using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using PortalProveedores.Mobile.Models;

namespace PortalProveedores.Mobile.Services
{
    /// <summary>
    /// Servicio encargado de la inicialización y acceso a la base de datos local (SQLite).
    /// </summary>
    public class DatabaseService
    {
        // SQLiteAsyncConnection permite operaciones no bloqueantes
        private SQLiteAsyncConnection? _database;

        // Nombre del archivo de la DB
        private const string DatabaseFilename = "PortalProveedores.db3";

        // Método para obtener la ruta del archivo en el dispositivo
        private static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        private async Task Init()
        {
            // Si ya está inicializada, salir.
            if (_database != null) return;

            // Se inicia la conexión
            _database = new SQLiteAsyncConnection(DatabasePath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            // Se crea la tabla si no existe
            await _database.CreateTableAsync<RemitoLocal>();

            // Nota de Seguridad:
            // Para SQLCipher, la conexión aquí incluiría la clave de encriptación
            // _database = new SQLiteAsyncConnection(DatabasePath, key: "clave-secreta");
        }

        // --- Operaciones CRUD para RemitoLocal ---

        public async Task<List<RemitoLocal>> GetRemitosPendientesAsync()
        {
            await Init();
            // Obtiene todos los remitos que no se han sincronizado (Sincronizado = false)
            return await _database!.Table<RemitoLocal>().Where(r => r.Sincronizado == false).ToListAsync();
        }

        public async Task<int> SaveRemitoAsync(RemitoLocal remito)
        {
            await Init();
            if (remito.IdLocal != 0)
            {
                // Actualizar un remito existente
                return await _database!.UpdateAsync(remito);
            }
            else
            {
                // Insertar un nuevo remito
                return await _database!.InsertAsync(remito);
            }
        }

        public async Task<int> DeleteRemitoAsync(RemitoLocal remito)
        {
            await Init();
            return await _database!.DeleteAsync(remito);
        }
    }
}
