public class Producto
{
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public int Existencia { get; private set; }

    public Producto(string codigo, string nombre, decimal precio, int existencia)
    {
        Codigo = codigo;
        Nombre = nombre;
        Precio = precio;
        Existencia = existencia;
    }

    public bool Vender(int cantidad)
    {
        if (cantidad <= 0 || cantidad > Existencia)
            return false;

        Existencia -= cantidad;
        return true;
    }

    public void Reabastecer(int cantidad)
    {
        Existencia += cantidad;
    }

    public int ConsultarExistencia()
    {
        return Existencia;
    }

    public bool CambiarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0)
            return false;

        Precio = nuevoPrecio;
        return true;
    }
}
