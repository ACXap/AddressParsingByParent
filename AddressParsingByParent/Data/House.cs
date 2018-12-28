namespace AddressParsingByParent
{
    public class House
    {
        public int GlobalID { get; set; }
        public int ParentID { get; set; }
        public int PosatalCode { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string NumberHouse { get; set; }

        public string Address { get; set; }

    }
}