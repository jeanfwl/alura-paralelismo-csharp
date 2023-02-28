﻿using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();

            BtnProcessar.IsEnabled = false;

            var contas = r_Repositorio.GetContaClientes();

            PgsProgress.Maximum = contas.Count();

            LimparView();

            var inicio = DateTime.Now;

            BtnCancelar.IsEnabled = true;

            var progresso = new Progress<string>(reporte =>
            {
                PgsProgress.Value++;
            });

            try
            {
                var resultado = await ConsolidarContas(contas, progresso, _cts.Token);

                var fim = DateTime.Now;
                AtualizarView(resultado, fim - inicio);
            }
            catch (OperationCanceledException)
            {
                TxtTempo.Text = "Operação cancelada pelo usuário";
            }
            finally
            {
                BtnProcessar.IsEnabled = true;
                BtnCancelar.IsEnabled = false;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            _cts.Cancel();
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> reportadorDeProgresso, CancellationToken ct)
        {
            var taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();

            var tasksContas = contas.Select(conta => 
                Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    var resultado = r_Servico.ConsolidarMovimentacao(conta, ct);

                    reportadorDeProgresso.Report(resultado);

                    ct.ThrowIfCancellationRequested();

                    return resultado;
                }, ct)
            );

            return await Task.WhenAll(tasksContas);
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
