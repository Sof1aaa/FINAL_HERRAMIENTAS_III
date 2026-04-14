using HERRAMIENTAS.Data;
using HERRAMIENTAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HERRAMIENTAS.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 LISTAR PEDIDOS
        public async Task<IActionResult> Index()
        {
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente);

            return View(await pedidos.ToListAsync());
        }

        // 🔍 DETALLE DEL PEDIDO
        public async Task<IActionResult> Details(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoProductos)
                    .ThenInclude(pp => pp.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // 🟢 CREAR (GET)
        public async Task<IActionResult> Create()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewBag.Productos = await _context.Productos
                .AsNoTracking()
                .ToListAsync(); 
            return View();
        }

        // 🔥 CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ClienteId, List<int> ProductoIds, List<int> Cantidades)
        {
            if (ProductoIds == null || Cantidades == null)
            {
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nombre");
                ViewBag.Productos = _context.Productos.ToList();
                return View();
            }

            var pedido = new Pedido
            {
                ClienteId = ClienteId,
                NumeroPedido = Guid.NewGuid().ToString().Substring(0, 8),
                Fecha = DateTime.Now,
                PedidoProductos = new List<PedidoProducto>()
            };

            decimal total = 0;

            for (int i = 0; i < ProductoIds.Count; i++)
            {
                var producto = await _context.Productos.FindAsync(ProductoIds[i]);

                if (producto != null && Cantidades[i] > 0)
                {
                    if (producto.Stock < Cantidades[i])
                    {
                        ModelState.AddModelError("", $"No hay suficiente stock para {producto.Nombre}");
                        return View();
                    }

                    producto.Stock -= Cantidades[i]; // 🔥 RESTAR STOCK

                    pedido.PedidoProductos.Add(new PedidoProducto
                    {
                        ProductoId = producto.Id,
                        Cantidad = Cantidades[i]
                    });

                    total += producto.Precio * Cantidades[i];
                }
            }

            pedido.Total = total;

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoProductos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewBag.Productos = _context.Productos.ToList();

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int ClienteId, int[] productosSeleccionados, Dictionary<int, int> cantidades)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoProductos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // 🔁 1. DEVOLVER STOCK ANTERIOR
            foreach (var pp in pedido.PedidoProductos)
            {
                var producto = await _context.Productos.FindAsync(pp.ProductoId);
                if (producto != null)
                {
                    producto.Stock += pp.Cantidad;
                }
            }

            // ❌ 2. ELIMINAR DETALLES
            _context.PedidoProductos.RemoveRange(pedido.PedidoProductos);

            pedido.ClienteId = ClienteId;
            pedido.PedidoProductos = new List<PedidoProducto>();

            decimal total = 0;

            // 🔥 3. AGREGAR NUEVOS Y RESTAR STOCK
            foreach (var productoId in productosSeleccionados)
            {
                var producto = await _context.Productos.FindAsync(productoId);

                if (producto != null)
                {
                    int cantidad = cantidades.ContainsKey(productoId) ? cantidades[productoId] : 1;

                    // 🚨 VALIDAR STOCK
                    if (producto.Stock < cantidad)
                    {
                        ModelState.AddModelError("", $"No hay suficiente stock para {producto.Nombre}");
                        return View(pedido);
                    }

                    producto.Stock -= cantidad; // 🔥 RESTAR STOCK NUEVO

                    pedido.PedidoProductos.Add(new PedidoProducto
                    {
                        ProductoId = producto.Id,
                        Cantidad = cantidad
                    });

                    total += producto.Precio * cantidad;
                }
            }

            // 💰 TOTAL
            pedido.Total = total;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}