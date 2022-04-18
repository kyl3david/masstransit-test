namespace Company.Contracts
{
    public record RabbitMessage
    {
        public int Count { get; set; }
        public string Value { get; init; }
    }
}
