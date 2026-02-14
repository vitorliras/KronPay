namespace Application.DTOs
{
    public partial class EntityResponse<T>
    {
        public string Success { get; set; }
        public string Message { get; set; }
        public T Model { get; set; }
    }
}
