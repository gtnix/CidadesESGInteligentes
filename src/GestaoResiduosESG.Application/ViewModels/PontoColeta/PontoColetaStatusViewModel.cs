namespace GestaoResiduosESG.Application.ViewModels.PontoColeta;

public class PontoColetaStatusViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public decimal CapacidadeMaxima { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public decimal PercentualOcupacao { get; set; }
    public string StatusOcupacao { get; set; } = string.Empty;
    public int TotalAlertasPendentes { get; set; }
}
