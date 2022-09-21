namespace acme.Models
{
    public class Encuesta
    {
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public List<Campo> campos { get; set; }
    }
}
