namespace Azure.MediaServices.Core.EncodingReservedUnitTypes
{
  public class EncodingReservedUnitType
  {
    public string AccountId { get; set; }
    public int ReservedUnitType { get; set; }
    public int MaxReservableUnits { get; set; }
    public int CurrentReservedUnits { get; set; }
  }
}
