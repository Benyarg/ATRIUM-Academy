using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Domain.Models;

public class Usuario : IdentityUser
{
    [Required(ErrorMessage = "Los nombres son obligatorios."), StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios."), StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria."), StringLength(220)]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria."), DataType(DataType.Date)]
    public DateTime FechaDeNacimiento { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<CarroCompras> CarritoCompras { get; set; } = new List<CarroCompras>();
    public ICollection<ProgresoEstudiante> Progresos { get; set; } = new List<ProgresoEstudiante>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<ConsultaContacto> ConsultasContacto { get; set; } = new List<ConsultaContacto>();
}
