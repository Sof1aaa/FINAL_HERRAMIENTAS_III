namespace HERRAMIENTAS.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public string NumeroPedido { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public decimal Total { get; set; }

        public List<PedidoProducto> PedidoProductos { get; set; }
    }
}