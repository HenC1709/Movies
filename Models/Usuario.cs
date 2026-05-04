namespace Movies.Models
{
    public class Usuario
{
    public string Nombre {get; set;} ="";
    public string ID {get; set;} ="";

    public Rol Rol { get; set; } = Rol.Usuario;
 }
 }