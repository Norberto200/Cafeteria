using Microsoft.EntityFrameworkCore;

public class Cafeteria
{
    private readonly CafeteriaDbContext _db;
    private readonly object _lock = new();

    public Cafeteria(CafeteriaDbContext db)
    {
        _db = db;
        _db.Database.EnsureCreated();
    }

    public bool HayProductos()
    {
        return _db.Productos.Any();
    }

    private ProductoEntity BuscarProducto(string codigo)
    {
        return _db.Productos.FirstOrDefault(p => p.Codigo == codigo);
    }

    public bool RegistrarProducto(string codigo, string nombre, decimal precio, int existencia)
    {
        lock (_lock)
        {
            if (BuscarProducto(codigo) != null) return false;
            var nuevo = new ProductoEntity
            {
                Codigo = codigo,
                Nombre = nombre,
                Precio = precio,
                Existencia = existencia
            };
            _db.Productos.Add(nuevo);
            _db.SaveChanges();
            return true;
        }
    }

    public int ConsultarExistencia(string codigo)
    {
        lock (_lock)
        {
            var p = BuscarProducto(codigo);
            return p != null ? p.Existencia : -1;
        }
    }

    public bool VenderProducto(string codigo, int cantidad)
    {
        lock (_lock)
        {
            var p = BuscarProducto(codigo);
            if (p == null || cantidad <= 0 || cantidad > p.Existencia)
                return false;

            p.Existencia -= cantidad;
            _db.Transacciones.Add(new TransaccionEntity
            {
                Tipo = "Venta",
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Cantidad = cantidad
            });
            _db.SaveChanges();
            return true;
        }
    }

    public bool ReabastecerProducto(string codigo, int cantidad)
    {
        lock (_lock)
        {
            if (cantidad <= 0) return false;
            var p = BuscarProducto(codigo);
            if (p == null) return false;

            p.Existencia += cantidad;
            _db.Transacciones.Add(new TransaccionEntity
            {
                Tipo = "Reabastecimiento",
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Cantidad = cantidad
            });
            _db.SaveChanges();
            return true;
        }
    }

    public bool CambiarPrecioProducto(string codigo, decimal nuevoPrecio)
    {
        lock (_lock)
        {
            var p = BuscarProducto(codigo);
            if (p == null || nuevoPrecio <= 0) return false;

            decimal anterior = p.Precio;
            p.Precio = nuevoPrecio;
            _db.Transacciones.Add(new TransaccionEntity
            {
                Tipo = "CambioPrecio",
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Cantidad = 0,
                PrecioAnterior = anterior,
                PrecioNuevo = nuevoPrecio
            });
            _db.SaveChanges();
            return true;
        }
    }

    public List<ProductoData> ObtenerProductos()
    {
        lock (_lock)
        {
            return _db.Productos.Select(p => new ProductoData
            {
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Existencia = p.Existencia
            }).ToList();
        }
    }

    public ProductoData ObtenerProducto(string codigo)
    {
        lock (_lock)
        {
            var p = BuscarProducto(codigo);
            if (p == null) return null;
            return new ProductoData
            {
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Existencia = p.Existencia
            };
        }
    }

    public List<Transaccion> ObtenerTransacciones()
    {
        lock (_lock)
        {
            return _db.Transacciones.OrderByDescending(t => t.Fecha).Select(t => new Transaccion
            {
                Tipo = t.Tipo,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                Cantidad = t.Cantidad,
                PrecioAnterior = t.PrecioAnterior,
                PrecioNuevo = t.PrecioNuevo,
                Fecha = t.Fecha
            }).ToList();
        }
    }

    public object ObtenerReportes()
    {
        lock (_lock)
        {
            var productos = _db.Productos.ToList();
            int totalStock = 0;
            decimal totalValor = 0;
            ProductoEntity masCaro = null;
            ProductoEntity masStock = null;

            foreach (var p in productos)
            {
                totalStock += p.Existencia;
                totalValor += p.Existencia * p.Precio;
                if (masCaro == null || p.Precio > masCaro.Precio) masCaro = p;
                if (masStock == null || p.Existencia > masStock.Existencia) masStock = p;
            }

            return new
            {
                totalProductos = productos.Count,
                totalStock,
                totalValor,
                productoMasCaro = masCaro?.Nombre,
                productoMasCaroPrecio = masCaro?.Precio,
                productoMasStock = masStock?.Nombre,
                productoMasStockCantidad = masStock?.Existencia,
                transaccionesRecientes = _db.Transacciones
                    .OrderByDescending(t => t.Fecha)
                    .Take(20)
                    .Select(t => new Transaccion
                    {
                        Tipo = t.Tipo,
                        Codigo = t.Codigo,
                        Nombre = t.Nombre,
                        Cantidad = t.Cantidad,
                        PrecioAnterior = t.PrecioAnterior,
                        PrecioNuevo = t.PrecioNuevo,
                        Fecha = t.Fecha
                    }).ToList()
            };
        }
    }

    public bool EliminarProducto(string codigo)
    {
        lock (_lock)
        {
            var p = BuscarProducto(codigo);
            if (p == null) return false;

            _db.Productos.Remove(p);
            _db.SaveChanges();
            return true;
        }
    }
}

public class ProductoData
{
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int Existencia { get; set; }
}

public class Transaccion
{
    public string Tipo { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public int Cantidad { get; set; }
    public decimal? PrecioAnterior { get; set; }
    public decimal? PrecioNuevo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
}
