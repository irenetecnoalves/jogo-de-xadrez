using tabuleiro;
using xadrez;
using System.Collections.Generic;


namespace xadrez
{
    class PartidadeXadrez
    {

        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }

        public bool terminada { get; set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidadeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executarMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQteMovimento();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            } 
            if(p is Rei && destino.coluna == origem.coluna +2){
                Posicao origemT = new Posicao(origem.linha, origem.coluna +3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna +1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimento();
                tab.colocarPeca(T,destinoT);
            }
            // jogada especial roque grande

            if(p is Rei && destino.coluna == origem.coluna -2){
                Posicao origemT = new Posicao(origem.linha, origem.coluna -4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna -1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimento();
                tab.colocarPeca(T,destinoT);
            }
            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQteMovimento();
            if (pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);
            // jogada especial roque pequeno
            if(p is Rei && destino.coluna == origem.coluna +2){
                Posicao origemT = new Posicao(origem.linha, origem.coluna +3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna +1);
                Peca T = tab.retirarPeca(destinoT);
                T.decrementarQteMovimento();
                tab.colocarPeca(T,origemT);
            }

            // jogada especial roque grande
            if(p is Rei && destino.coluna == origem.coluna -2){
                Posicao origemT = new Posicao(origem.linha, origem.coluna -4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna -1);
                Peca T = tab.retirarPeca(destinoT);
                T.decrementarQteMovimento();
                tab.colocarPeca(T,origemT);
            }
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executarMovimento(origem, destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("voce n??o pode se colocar em xeque!");
            }
            if (estaEmXeque(adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }

            if (testeXequeMate(adversaria(jogadorAtual)))
            {
                terminada = true;

            }
            else
            {
                turno++;
                mudaJogador();

            }

        }

        public void validarPosicaoOrigem(Posicao pos)
        {
            if (tab.peca(pos) == null)
            {
                throw new TabuleiroException(" n??o existe peca na posi????o original escolhida.");

            }
            if (jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A pe??a de origem escolhida n??o e sua");
            }
            if (!tab.peca(pos).existeMoviventosPossiveis())
            {
                throw new TabuleiroException("N??o h?? movimentos possiveis para esta pe??a");
            }
        }

        public void validarPosicaoDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).movimentoPossivel(destino))
            {
                throw new TabuleiroException("Posi????o de destino inv??lida.");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual == Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> pecaCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;

        }

        public HashSet<Peca> pecasemJogo(Cor cor)
        {

            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecaCapturadas(cor));
            return aux;


        }

        private Cor adversaria(Cor cor)
        {
            if (cor == Cor.Branca)
            {
                return Cor.Preta;

            }
            else
            {
                return Cor.Branca;
            }
        }

        private Peca rei(Cor cor)
        {
            foreach (Peca x in pecasemJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }
        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);
            if (R == null)
            {
                throw new TabuleiroException("Na tem rei da cor " + cor + "no tabuleiro!");
            }

            foreach (Peca x in pecasemJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if (mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequeMate(Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in pecasemJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for (int i = 0; i < tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {

                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executarMovimento(origem, destino);
                            bool testeXequeMate = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXequeMate)
                            {
                                return false;
                            }
                        }
                    }

                }
            }
            return true;
        }




        public void colocarNovaPeca(char coluna, int linha, Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }


        private void colocarPecas()
        {

            colocarNovaPeca('a', 1, new Torre(tab, Cor.Branca));
            colocarNovaPeca('b', 1, new Peao(tab, Cor.Branca));
            colocarNovaPeca('c', 1, new Torre(tab, Cor.Branca));
            colocarNovaPeca('d', 1, new Rei(tab, Cor.Branca,this));
            colocarNovaPeca('e', 1, new Torre(tab, Cor.Branca));
            colocarNovaPeca('f', 1, new Rei(tab, Cor.Branca,this));
            colocarNovaPeca('g', 1, new Rei(tab, Cor.Branca,this));
            colocarNovaPeca('h', 1, new Torre(tab, Cor.Branca));           
            colocarNovaPeca('a', 2, new Torre(tab, Cor.Branca));
            colocarNovaPeca('b', 2, new Peao(tab, Cor.Branca));
            colocarNovaPeca('c', 2, new Torre(tab, Cor.Branca));
            colocarNovaPeca('d', 2, new Peao(tab, Cor.Branca));
            colocarNovaPeca('e', 2, new Torre(tab, Cor.Branca));
            colocarNovaPeca('f', 2, new Rei(tab, Cor.Branca,this));
            colocarNovaPeca('g', 2, new Torre(tab, Cor.Branca));
            colocarNovaPeca('h', 2, new Rei(tab, Cor.Branca,this));


            colocarNovaPeca('a', 8, new Torre(tab, Cor.Preta));
            colocarNovaPeca('b', 8, new Peao(tab, Cor.Preta));
            colocarNovaPeca('c', 8, new Torre(tab, Cor.Preta));
            colocarNovaPeca('d', 8, new Rei(tab, Cor.Preta, this));
            colocarNovaPeca('e', 8, new Torre(tab, Cor.Preta));
            colocarNovaPeca('f', 8, new Rei(tab, Cor.Preta, this));
            colocarNovaPeca('g', 8, new Rei(tab, Cor.Preta,this));
            colocarNovaPeca('h', 8, new Torre(tab, Cor.Preta));           
            colocarNovaPeca('a', 7, new Torre(tab, Cor.Preta));
            colocarNovaPeca('b', 7, new Peao(tab, Cor.Preta));
            colocarNovaPeca('c', 7, new Torre(tab, Cor.Preta));
            colocarNovaPeca('d', 7, new Peao(tab, Cor.Preta));
            colocarNovaPeca('e', 7, new Torre(tab, Cor.Preta));
            colocarNovaPeca('f', 7, new Rei(tab, Cor.Preta,this));
            colocarNovaPeca('g', 7, new Torre(tab, Cor.Preta));
            colocarNovaPeca('h', 7, new Rei(tab, Cor.Preta,this));




        }

    }
}