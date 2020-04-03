

namespace Cambios
{

    using Modelos;
    using Cambios.Servicos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Drawing;

    public partial class Form1 : Form
    {
        #region Atributos
        private List<Rate> Rates;
        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;
        #endregion

       

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;
           
            LabelResultado.Text = "A atualizar taxas...";

            var connection = networkService.CheckConnection();

            if (!connection.IsSuccess)
            {
                
                LoadLocalRates();
                load=false;
            }
            else
            {
                //Carrega os dados da api
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                LabelResultado.ForeColor = Color.Red;
                LabelStatus.ForeColor = Color.Red;
                LabelResultado.Text = "Não há ligação á Internet" + Environment.NewLine +
                    "e não foram prévimente carregadas as taxas." + Environment.NewLine +
                    "Tente mais tarde! ";
                LabelStatus.Text = "Primeira inicialização deverá ter a ligação á Internet.";
                return;
            }
            ComboBoxOrigem.DataSource = Rates;
            ComboBoxOrigem.DisplayMember = "Name";

            //BindingContext é a Classe que nos liga os objetos do interface ao codigo
            //Corrige bug da microsoft -- faz o binding de duas combobox diferentes
            ComboBoxDestino.BindingContext=new BindingContext();
            ComboBoxDestino.DataSource = Rates;
            ComboBoxDestino.DisplayMember = "Name";

           
            ButtonConverter.Enabled = true;
            ButtonTroca.Enabled = true;
            LabelResultado.Text = "Taxas Atualizadas...";

            if (load)
            {
                LabelStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                LabelStatus.Text = string.Format("Taxas carregadas da Base de Dados ");

            }
            ProgressBar1.Value = 100;
        }

        private void LoadLocalRates()
        {
            Rates=dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            ProgressBar1.Value = 0;
            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates");
            Rates =(List<Rate>) response.Result;

            dataService.DeleteData();
            dataService.SaveData(Rates);
        }

        private void ButtonConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(TextBoxValor.Text))
            {
                dialogService.ShowMessage("Erro", "Insira um valor a converter");
                return;
            }
            decimal valor;
            if (!decimal.TryParse(TextBoxValor.Text, out valor))
            {
                dialogService.ShowMessage("Erro de conversão","Valor terá que ser numérico");
                return;
            }
            if (ComboBoxOrigem.SelectedItem==null)
            {
                dialogService.ShowMessage("Erro","Tem que escolher uma moeda a converter");
                return;
            }
            if (ComboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda de destino para converter");
                return;
            }

            var taxaOrigem =(Rate) ComboBoxOrigem.SelectedItem;
            var taxaDestino = (Rate)ComboBoxDestino.SelectedItem;
           
            var valorConvertido= valor/(decimal) taxaOrigem.TaxRate* (decimal) taxaDestino.TaxRate;

            LabelResultado.Text = string.Format("{0}  {1:F2} = {2} {3:F2}", taxaOrigem.Code, valor, taxaDestino.Code, valorConvertido);

        }

        private void ButtonTroca_Click(object sender, EventArgs e)
        {
            Trocar();
        }

        private void Trocar()
        {
            var aux = ComboBoxOrigem.SelectedItem;

            ComboBoxOrigem.SelectedItem = ComboBoxDestino.SelectedItem;
            ComboBoxDestino.SelectedItem = aux;
            Converter();
        }

        private void TextBoxValor_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxValor.Text))
            {
                Converter();
            }
           
        }
    }
}
