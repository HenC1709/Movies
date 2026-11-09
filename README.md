# Movies

Aplicación de consola en C# para administrar una colección de películas. Permite registrar usuarios, agregar películas, buscarlas por título y consultar la lista guardada.

## Requisitos

- .NET 10 SDK

## Ejecutar

Desde la carpeta del proyecto:

```bash
dotnet run --project Movies.csproj
```

## Opciones del menú

- Registrar un usuario. El código de acceso debe tener cuatro dígitos.
- Buscar una película por título.
- Agregar una película indicando su título y autor.
- Mostrar las películas guardadas.
- Salir.

## Persistencia

Los datos actuales se guardan en archivos JSON dentro de `Data/`:

- `Usuarios.json`: usuarios registrados.
- `PeliculasGuardadas.json`: películas y fecha de ingreso.

La persistencia SQL todavía no está implementada. `Services/Sql/` contiene plantillas comentadas para orientar una futura migración.

## Estructura

- `Models/`: modelos de usuario, rol y película.
- `Services/`: registro, búsqueda y persistencia actual en JSON.
- `Logic/`: lista de películas en memoria.
- `UI/`: menú de consola.
- `Data/`: archivos JSON de datos.
