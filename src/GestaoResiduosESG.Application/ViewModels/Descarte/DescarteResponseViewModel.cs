namespace GestaoResiduosESG.Application.ViewModels.Descarte;

public class DescarteResponseViewModel
{
    public int Id { get; set; }
    public int ResiduoId { get; set; }
    public string NomeResiduo { get; set; } = string.Empty;
    public int PontoColetaId { get; set; }
    public string NomePontoColeta { get; set; } = string.Empty;
    public DateTime DataDescarte { get; set; }
    public decimal Quantidade { get; set; }
    public string Responsavel { get; set; } = string.Empty;
}
