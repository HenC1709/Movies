# Movies

Videoteca de consola en C# y .NET 10. El catálogo es compartido y cada cuenta tiene su propia lista de películas pendientes y vistas.

## Ejecutar

Desde la carpeta del proyecto, ejecuta `dotnet run --project Movies.csproj`. La primera compilación descarga de NuGet el proveedor de SQLite.

## Primer inicio

Movies crea `Movies.db` en la carpeta local de datos de la aplicación (en Windows, `%LOCALAPPDATA%\Movies`). Puedes configurar otra carpeta mediante `MOVIES_DATA_DIR`, por ejemplo para usar una base de práctica independiente.

El primer inicio importa el archivo `Data/PeliculasGuardadas.json` distribuido con la aplicación. La importación conserva títulos, autores, fechas y desfases horarios, guarda una copia como `CatalogoImportado.json` junto a la base y muestra el total importado. Un registro de migración evita duplicar películas al reiniciar. Si una fila es inválida, se revierte la transacción. Corrige el JSON de origen y vuelve a ejecutar el proyecto.

## Cuentas y catálogo

Registra la primera cuenta en una instalación local de confianza. Esa cuenta será administradora y podrá agregar y editar películas. Las cuentas posteriores tendrán el rol Usuario. Usa una contraseña nueva de al menos ocho caracteres. Los usuarios del antiguo JSON eran datos de práctica y deberán registrarse de nuevo. SQLite guarda el hash PBKDF2 y la sal individual de cada contraseña.

Como invitado puedes buscar y listar películas. Con una sesión abierta, consulta el ID de una película en el catálogo o en los resultados, elige **Marcar película** y selecciona **Pendiente** o **Vista**. Para cambiar el estado repite la operación con el mismo ID. **Mi lista** muestra las selecciones de la cuenta activa.

## Datos locales

SQLite conserva el catálogo, las cuentas y las listas entre sesiones. Para respaldar todos los datos, cierra las instancias de Movies y copia la carpeta de datos. El JSON de importación solo contiene el catálogo original.
