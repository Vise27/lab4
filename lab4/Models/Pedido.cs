namespace lab4.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public List<string> Productos { get; set; }
        public string Direccion { get; set; }
        public string Estado { get; set; }

    }
}
