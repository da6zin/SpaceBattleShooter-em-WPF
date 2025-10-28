using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Space_battle_shooter_WPF_MOO_ICT
{
    public partial class MainWindow : Window
    {
        
        DispatcherTimer timerJogo = new DispatcherTimer();
        
        bool moverEsquerda, moverDireita;
        
        List<Rectangle> itensParaRemover = new List<Rectangle>();
        
        Random aleatorio = new Random();

        
        int contadorInimigos = 100;
        int velocidadeJogador = 10;
        int limite = 50;
        int pontuacao = 0;
        int dano = 0;
        int velocidadeInimigo = 10;
        int contadorPowerUp = 600;

        private bool poderEspecialPronto = false;
        private bool poderEspecialAtivo = false;
        private int recargaPoderEspecial = 750;
        private int timerPoderEspecial = 0;
        private int duracaoPoderEspecial = 250;
        private int timerPoderEspecialAtivo = 0;
        private int velocidadeInimigoOriginal;

        Rect hitboxJogador;

        public MainWindow()
        {
            InitializeComponent();

            // Configura e inicia o motor do jogo (o timer).
            timerJogo.Interval = TimeSpan.FromMilliseconds(20);
            timerJogo.Tick += LoopJogo;
            timerJogo.Start();

            MeuCanvas.Focus();

            // Imagem de fundo
            ImageBrush fundoTela = new ImageBrush();
            fundoTela.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/purple2.jpg"));
            fundoTela.TileMode = TileMode.Tile;
            fundoTela.Viewport = new Rect(0, 0, 0.15, 0.15);
            fundoTela.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            MeuCanvas.Background = fundoTela;

            // Imagem da nave do jogador.
            ImageBrush imagemJogador = new ImageBrush();
            imagemJogador.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/player.png"));
            jogador.Fill = imagemJogador;
        }

        private void LoopJogo(object sender, EventArgs e)
        {
            hitboxJogador = new Rect(Canvas.GetLeft(jogador), Canvas.GetTop(jogador), jogador.Width, jogador.Height);
            contadorInimigos -= 1;
            contadorPowerUp -= 1;

            textoPontuacao.Content = "Pontos: " + pontuacao;
            textoDano.Content = "Dano: " + dano;

            GerenciarPoderEspecial();

            if (contadorInimigos < 0)
            {
                CriarInimigos();
                contadorInimigos = limite;
            }

            // Mostra o power-up na tela.
            if (contadorPowerUp < 0 && ferramentaPowerUp.Visibility == Visibility.Collapsed)
            {
                Canvas.SetLeft(ferramentaPowerUp, aleatorio.Next(30, 430));
                Canvas.SetTop(ferramentaPowerUp, aleatorio.Next(50, 400));
                ferramentaPowerUp.Visibility = Visibility.Visible;
                contadorPowerUp = aleatorio.Next(800, 1200);
            }

            // Move o jogador
            if (moverEsquerda == true && Canvas.GetLeft(jogador) > 0)
            {
                Canvas.SetLeft(jogador, Canvas.GetLeft(jogador) - velocidadeJogador);
            }
            if (moverDireita == true && Canvas.GetLeft(jogador) + 90 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(jogador, Canvas.GetLeft(jogador) + velocidadeJogador);
            }

            foreach (var elemento in MeuCanvas.Children.OfType<Rectangle>())
            {
                if (elemento is Rectangle && (string)elemento.Tag == "projetil")
                {
                    Canvas.SetTop(elemento, Canvas.GetTop(elemento) - 20);
                    Rect hitboxProjetil = new Rect(Canvas.GetLeft(elemento), Canvas.GetTop(elemento), elemento.Width, elemento.Height);

                    if (Canvas.GetTop(elemento) < 10)
                    {
                        itensParaRemover.Add(elemento);
                    }

                    foreach (var outroElemento in MeuCanvas.Children.OfType<Rectangle>())
                    {
                        if (outroElemento is Rectangle && (string)outroElemento.Tag == "inimigo")
                        {
                            Rect hitboxInimigo = new Rect(Canvas.GetLeft(outroElemento), Canvas.GetTop(outroElemento), outroElemento.Width, outroElemento.Height);
                            if (hitboxProjetil.IntersectsWith(hitboxInimigo))
                            {
                                itensParaRemover.Add(elemento);
                                itensParaRemover.Add(outroElemento);
                                pontuacao++;
                            }
                        }
                        // Se colidiu com power-up, esconde o power-up e "cura" o jogador.
                        if (outroElemento is Rectangle && (string)outroElemento.Tag == "powerup")
                        {
                            if (outroElemento.Visibility == Visibility.Visible)
                            {
                                Rect hitboxPowerUp = new Rect(Canvas.GetLeft(outroElemento), Canvas.GetTop(outroElemento), outroElemento.Width, outroElemento.Height);
                                if (hitboxProjetil.IntersectsWith(hitboxPowerUp))
                                {
                                    itensParaRemover.Add(elemento);
                                    outroElemento.Visibility = Visibility.Collapsed;
                                    dano -= 20;
                                    if (dano < 0) { 
                                        dano = 0;
                                    }
                                }
                            }
                        }
                    }
                }

                if (elemento is Rectangle && (string)elemento.Tag == "inimigo")
                {
                    Canvas.SetTop(elemento, Canvas.GetTop(elemento) + velocidadeInimigo);

                    if (Canvas.GetTop(elemento) > 750)
                    {
                        itensParaRemover.Add(elemento);
                        dano += 10;
                    }
                    Rect hitboxInimigo = new Rect(Canvas.GetLeft(elemento), Canvas.GetTop(elemento), elemento.Width, elemento.Height);
                    if (hitboxJogador.IntersectsWith(hitboxInimigo))
                    {
                        itensParaRemover.Add(elemento);
                        dano += 5;
                    }
                }
            }

            foreach (Rectangle item in itensParaRemover)
            {
                MeuCanvas.Children.Remove(item);
            }
            itensParaRemover.Clear();

            // Aumenta a dificuldade do jogo com base na pontuação.
            if (!poderEspecialAtivo)
            {
                if (pontuacao > 7)
                {
                    limite = 20;
                    velocidadeInimigo = 15;
                    velocidadeJogador = 14;
                }
                if (pontuacao > 30)
                {
                    limite = 10;
                    velocidadeInimigo = 23;
                    velocidadeJogador = 20;
                }
            }

            if (dano > 99)
            {
                timerJogo.Stop();
                MessageBox.Show("Capitão, você destruiu " + pontuacao + " naves alienígenas" + Environment.NewLine + "Pressione Ok para jogar de novo", "Fim de Jogo");
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void AoPressionarTecla(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) moverEsquerda = true;
            if (e.Key == Key.Right) moverDireita = true;
        }

        private void AoSoltarTecla(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Left) moverEsquerda = false;
            if (e.Key == Key.Right) moverDireita = false;

            if (e.Key == Key.Space)
            {
                Rectangle novoProjetil = new Rectangle
                {
                    Tag = "projetil",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                Canvas.SetLeft(novoProjetil, Canvas.GetLeft(jogador) + jogador.Width / 2);
                Canvas.SetTop(novoProjetil, Canvas.GetTop(jogador) - novoProjetil.Height);
                MeuCanvas.Children.Add(novoProjetil);
            }

            // Ativa o poder especial ao soltar a tecla B.
            if (e.Key == Key.B && poderEspecialPronto)
            {
                poderEspecialAtivo = true;
                timerPoderEspecialAtivo = duracaoPoderEspecial;
                velocidadeInimigoOriginal = velocidadeInimigo;
                velocidadeInimigo = 2; // rEduz
            }
        }

        private void GerenciarPoderEspecial()
        {
            if (!poderEspecialPronto && !poderEspecialAtivo)
            {
                timerPoderEspecial++;
                double porcentagem = (double)timerPoderEspecial / recargaPoderEspecial;
                preenchimentoBarraPoderEspecial.Width = porcentagem * fundoBarraPoderEspecial.Width;

                if (timerPoderEspecial >= recargaPoderEspecial)
                {
                    poderEspecialPronto = true;
                    statusPoderEspecial.Content = "Pronto!";
                    statusPoderEspecial.Foreground = Brushes.Green;
                }
            }

            if (poderEspecialAtivo)
            {
                timerPoderEspecialAtivo--;

                if (timerPoderEspecialAtivo <= 0)
                {
                    poderEspecialAtivo = false;
                    velocidadeInimigo = velocidadeInimigoOriginal;
                    timerPoderEspecial = 0;
                    preenchimentoBarraPoderEspecial.Width = 0;
                    statusPoderEspecial.Content = "Carregando...";
                    statusPoderEspecial.Foreground = Brushes.LightGray;
                }
            }
        }

        private void CriarInimigos()
        {
            ImageBrush spriteInimigo = new ImageBrush();
            int contadorSpriteInimigo = aleatorio.Next(1, 6);
            switch (contadorSpriteInimigo)
            {
                case 1: spriteInimigo.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/1.png")); break;
                case 2: spriteInimigo.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/2.png")); break;
                case 3: spriteInimigo.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/3.png")); break;
                case 4: spriteInimigo.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/4.png")); break;
                case 5: spriteInimigo.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/asteroide.png")); break;
            }

                Rectangle novoInimigo = new Rectangle
            {
                Tag = "inimigo",
                Height = 50,
                Width = 56,
                Fill = spriteInimigo
            };

            Canvas.SetTop(novoInimigo, -100);
            Canvas.SetLeft(novoInimigo, aleatorio.Next(30, 430));
            MeuCanvas.Children.Add(novoInimigo);
        }
    }
}


