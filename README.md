# Movies

Videoteca de consola en C# y .NET 10. El catálogo es compartido y cada cuenta tiene su propia lista de películas pendientes y vistas.

## Ejecutar

Desde la carpeta del proyecto, ejecuta `dotnet run --project Movies.csproj`. La primera compilación descarga de NuGet el proveedor de SQLite.

Para abrir Movies en WPF desde Windows, ejecuta `dotnet run --project Desktop/Movies.Desktop.csproj`. Usa la barra superior para buscar por título. Pulsa **Catálogo** para limpiar el filtro y volver a ver todas las películas. Desde **Usuario** puedes iniciar sesión con una cuenta de consola o registrar una nueva. Ambas aplicaciones usan la misma biblioteca y la misma base local, y respetan `MOVIES_DATA_DIR`.

## Primer inicio

Movies crea `Movies.db` en la carpeta local de datos de la aplicación (en Windows, `%LOCALAPPDATA%\Movies`). Puedes configurar otra carpeta mediante `MOVIES_DATA_DIR`, por ejemplo para usar una base de práctica independiente.

El primer inicio importa el archivo `Data/PeliculasGuardadas.json` distribuido con la aplicación. La importación conserva títulos, autores, fechas y desfases horarios, guarda una copia como `CatalogoImportado.json` junto a la base y muestra el total importado. Un registro de migración evita duplicar películas al reiniciar. Si una fila es inválida, se revierte la transacción. Corrige el JSON de origen y vuelve a ejecutar el proyecto.

## Cuentas y catálogo

Registra la primera cuenta en una instalación local de confianza. Esa cuenta será administradora y podrá agregar y editar películas. Las cuentas posteriores tendrán el rol Usuario. Usa una contraseña nueva de al menos ocho caracteres. Los usuarios del antiguo JSON eran datos de práctica y deberán registrarse de nuevo. SQLite guarda el hash PBKDF2 y la sal individual de cada contraseña.

Como invitado puedes buscar y listar películas. Con una sesión abierta, consulta el ID de una película en el catálogo o en los resultados, elige **Marcar película** y selecciona **Pendiente** o **Vista**. Para cambiar el estado repite la operación con el mismo ID. **Mi lista** muestra las selecciones de la cuenta activa.

En WPF, pasa el puntero sobre una carátula o enfócala con Tab para ver el título. Haz clic o pulsa Enter para abrir su ficha. Con una sesión abierta, puedes marcarla como **Pendiente** o **Vista** y consultar su estado en **Mi lista**, en la barra superior. Las cuentas administradoras tienen **Agregar película** en la barra inferior y **Editar** en la ficha. Al guardar, la barra inferior confirma el título.

En el formulario de administración, **Elegir imagen** permite adjuntar un JPEG o PNG de hasta 8 MiB y 4096 píxeles por lado. **Quitar carátula** borra la imagen cuando guardas. Movies conserva su propia copia dentro de SQLite, junto con el resto de los datos.

El catálogo, **Mi lista** y la ficha muestran la imagen guardada. Si la película no tiene imagen o Movies no puede leerla, aparece una carátula genérica.

### Buscar carátulas en TMDB

Configura `TMDB_READ_ACCESS_TOKEN` con tu **API Read Access Token** de [los ajustes de TMDB](https://www.themoviedb.org/settings/api). En PowerShell puedes cargarlo en la sesión actual antes de abrir Movies:

```powershell
$env:TMDB_READ_ACCESS_TOKEN = Read-Host 'API Read Access Token de TMDB' -MaskInput
dotnet run --project Desktop/Movies.Desktop.csproj
```

En el formulario de una película, escribe el título, pulsa **Buscar en TMDB** y elige un resultado por título y año. **Usar carátula** descarga la imagen y permite revisarla antes de pulsar **Guardar**. Después de guardar, la carátula se puede ver sin conexión.

## Datos locales

SQLite conserva el catálogo, las cuentas y las listas entre sesiones. Para respaldar todos los datos, cierra las instancias de Movies y copia la carpeta de datos. El JSON de importación solo contiene el catálogo original.
