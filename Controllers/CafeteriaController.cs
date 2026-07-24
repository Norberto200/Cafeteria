using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class CafeteriaController : ControllerBase
{
    private readonly Cafeteria _cafeteria;

    public CafeteriaController(Cafeteria cafeteria)
    {
        _cafeteria = cafeteria;
    }

    [HttpGet("productos")]
    public ActionResult<List<ProductoData>> GetProductos()
    {
        return Ok(_cafeteria.ObtenerProductos());
    }

    [HttpGet("producto/{codigo}")]
    public ActionResult<ProductoData> GetProducto(string codigo)
    {
        var producto = _cafeteria.ObtenerProducto(codigo);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado." });
        return Ok(producto);
    }

    [HttpPost("registrar")]
    public ActionResult Registrar([FromBody] RegistrarRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool resultado = _cafeteria.RegistrarProducto(request.Codigo, request.Nombre, request.Precio, request.Existencia);
        if (resultado)
            return Ok(new { mensaje = "Producto registrado." });
        return BadRequest(new { mensaje = "El código ya existe." });
    }

    [HttpPost("vender")]
    public ActionResult Vender([FromBody] CodigoCantidadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool resultado = _cafeteria.VenderProducto(request.Codigo, request.Cantidad);
        if (resultado)
            return Ok(new { mensaje = "Venta realizada." });
        return BadRequest(new { mensaje = "No se pudo realizar la venta." });
    }

    [HttpPost("reabastecer")]
    public ActionResult Reabastecer([FromBody] CodigoCantidadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool resultado = _cafeteria.ReabastecerProducto(request.Codigo, request.Cantidad);
        if (resultado)
            return Ok(new { mensaje = "Producto reabastecido." });
        return NotFound(new { mensaje = "Producto no encontrado." });
    }

    [HttpGet("reportes")]
    public ActionResult GetReportes()
    {
        return Ok(_cafeteria.ObtenerReportes());
    }

    [HttpPost("cambiarprecio")]
    public ActionResult CambiarPrecio([FromBody] CambiarPrecioRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool resultado = _cafeteria.CambiarPrecioProducto(request.Codigo, request.NuevoPrecio);
        if (resultado)
            return Ok(new { mensaje = "Precio actualizado." });
        return BadRequest(new { mensaje = "No se pudo cambiar el precio." });
    }

    [HttpDelete("producto/{codigo}")]
    public ActionResult Eliminar(string codigo)
    {
        bool resultado = _cafeteria.EliminarProducto(codigo);
        if (resultado)
            return Ok(new { mensaje = "Producto eliminado." });
        return NotFound(new { mensaje = "Producto no encontrado." });
    }
}

public class RegistrarRequest
{
    [Required] public string Codigo { get; set; }
    [Required] public string Nombre { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Precio { get; set; }
    [Range(0, int.MaxValue)] public int Existencia { get; set; }
}

public class CodigoCantidadRequest
{
    [Required] public string Codigo { get; set; }
    [Range(1, int.MaxValue)] public int Cantidad { get; set; }
}

public class CambiarPrecioRequest
{
    [Required] public string Codigo { get; set; }
    [Range(0.01, double.MaxValue)] public decimal NuevoPrecio { get; set; }
}
