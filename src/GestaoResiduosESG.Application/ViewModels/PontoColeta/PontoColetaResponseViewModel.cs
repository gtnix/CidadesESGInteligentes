namespace GestaoResiduosESG.Application.ViewModels.PontoColeta;

public class PontoColetaResponseViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public decimal CapacidadeMaxima { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public decimal PercentualOcupacao =>
        CapacidadeMaxima > 0 ? Math.Round((QuantidadeAtual / CapacidadeMaxima) * 100, 2) : 0;
}
