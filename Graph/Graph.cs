using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Graph
{
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }

    public class Graph<T> : IGraph<T>
    {
        private IEnumerable<ILink<T>> links { get; set; }
        private List<List<ILink<T>>> listaDeRotasPossiveis { get; set; }

        public Graph(IEnumerable<ILink<T>> links)
        {
            this.links = links;
            listaDeRotasPossiveis = new List<List<ILink<T>>>();
        }

        /// <summary>
        /// Calcula todas as rotas possíveis entre dois pontos de um grafo, desconsiderando repetições por um mesmo circuito
        /// </summary>
        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            IniciaPrimeirasRotas(source);

            //Será criada uma lista auxiliar para não perdermos o conteúdo original da variável "listaDeRotasPossiveis"
            var ListaAuxiliarDeRotas = new List<List<ILink<T>>>(listaDeRotasPossiveis);

            //A variável count será usada para controlarmos a rota em que estamos dentro da lista auxiliar de rotas
            var count = 0;

            while (true)
            {
                //Copiamos o conteúdo da variável "links" para uma variável auxiliar ("cloneLinks") de forma a
                //não perdermos o conteúdo original da primeira variável
                var cloneLinks = new List<ILink<T>>();
                cloneLinks = (List<ILink<T>>)links;

                //Nas linhas abaixo, buscamos todas as rotas que não chegaram ao destino final ("target")
                //Usamos a condicional para movermos de ramificação apenas quando a rota anterior for totalmente expandida
                if (ListaAuxiliarDeRotas.Count <= count)
                {
                    ListaAuxiliarDeRotas = listaDeRotasPossiveis.Where(x => !x.Last().Target.Equals(target)).ToList();
                    count = 0;
                }

                //Na linha abaixo, exploramos as próximas rotas ou ramificações de uma rota
                AdicionaProximosLinks(ListaAuxiliarDeRotas.ElementAt(count), cloneLinks, target);

                //A variável local count é incrementada para que seja explorada uma ramificação
                //diferente na próxima iteração
                count++;

                //Nas linhas abaixo, contamos quantas rotas já chegaram ao destino final desejado
                var countRotasFinalizadas = 0;
                foreach (var rota in listaDeRotasPossiveis)
                {
                    if (rota.Where(x => x.Target.Equals(target)).Any())
                        countRotasFinalizadas++;
                }

                //Se todas as rotas já chegaram ao destino final desejado, encerramos o laço
                if (countRotasFinalizadas == listaDeRotasPossiveis.Count)
                    break;
            }

            //Inicialmente, eu não havia preparado a solução para o retorno do tipo IObservable. As linhas abaixo
            //são para adaptar meu raciocínio inicial a esse tipo de retorno
            if (listaDeRotasPossiveis is List<List<ILink<string>>>)
            {
                var rotasEmString = new List<IEnumerable<T>>() as List<IEnumerable<string>>;

                foreach (var rota in listaDeRotasPossiveis)
                {
                    var rotaEmString = new List<T>() as List<string>;
                    var stringDeConcatenacao = string.Empty;
                    var countLinksDaRota = 0;
                    foreach (var link in rota)
                    {
                        stringDeConcatenacao = stringDeConcatenacao + link.ToString().Substring(0, 1) + "-";
                        countLinksDaRota++;
                        if (countLinksDaRota == rota.Count)
                            stringDeConcatenacao += link.ToString().Last().ToString();
                    }
                    rotaEmString.Add(stringDeConcatenacao);
                    rotasEmString.Add(rotaEmString);
                }

                return ((IEnumerable<IEnumerable<T>>)rotasEmString).ToObservable();
            }
            else
            {
                //Caso não seja passada "string" para T, a solução que eu propus precisaria continuar abaixo
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// Cria as primeiras rotas viáveis com base em cada possibilidade para o primeiro link
        /// </summary>
        private void IniciaPrimeirasRotas(T source)
        {
            //Na linha abaixo, removemos alguns links que provocariam passagens repetidas num mesmo circuito
            links = links.Where(x => !x.Target.Equals(source)).ToList();
            List<ILink<T>> possivelRota;

            //Na linha abaixo, calculamos os primeiros links possíveis, tendo como referência a
            //origem ("source") solicitada
            var primeirosLinks = links.Where(x => x.Source.Equals(source)).ToList();

            //Na linha abaixo, criamos as primeiras rotas e adicionamos um dos links possíveis em cada uma delas
            foreach (var link in primeirosLinks)
            {
                possivelRota = new List<ILink<T>>();
                possivelRota.Add(link);
                listaDeRotasPossiveis.Add(possivelRota);
            }
        }

        /// <summary>
        /// Insere o próximo nó de uma rota ou cria bifurcações (trifurcações, etc..) quando isso é possível
        /// </summary>
        private void AdicionaProximosLinks(List<ILink<T>> rotaAtual, List<ILink<T>> cloneLinks, T target)
        {
            //Em alguns casos, quando apenas um link é suficiente para conectar o ponto de partida e o de chegada,
            //não devemos buscar por ramificações porque elas provocariam duplicidades na resposta final
            if (rotaAtual.Last().Target.Equals(target))
                return;

            //Na linha abaixo, removemos alguns links que provocariam passagens repetidas num mesmo circuito
            //Usamos uma variável auxiliar ("cloneLinks") para que evitemos perder o conteúdo original de "links"
            cloneLinks.RemoveAll(x => x.Target.Equals(rotaAtual.Last().Source));

            //Na linha abaixo, calculamos os próximos links, com base no link atual (último link da rota)
            var proximosLinks = links.Where(x => x.Source.Equals(rotaAtual.Last().Target)).ToList();

            //Na linha abaixo, criamos um vetor com a quantidade necessária de listas para abrigar as
            //ramificações de um caminho
            var ramificacoesDaRotaAtual = new List<ILink<T>>[proximosLinks.Count];

            //A variável local "count" será usada para controlarmos qual ramificação é manipulada
            //dentro do vetor criado
            var count = 0;

            //Quando as ramificações são criadas, devemos apagar a rota originária para evitar duplicidades
            listaDeRotasPossiveis.RemoveAll(x => x.Last().Equals(rotaAtual.Last()));

            //No laço abaixo, inicializamos as listas do vetor e inserimos uma ramificação em cada uma delas
            foreach (var proximoLink in proximosLinks)
            {
                ramificacoesDaRotaAtual[count] = new List<ILink<T>>();
                ramificacoesDaRotaAtual[count] = rotaAtual.ToList();
                ramificacoesDaRotaAtual[count].Add(proximoLink);
                listaDeRotasPossiveis.Add(ramificacoesDaRotaAtual[count]);
                count++;
            }

        }

    }

}