using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            string[] lines = File.ReadAllLines(((FileStream)input).Name);
            CriaListaDeBlocosDeLegendas(lines, out List<List<string>> listaDeBlocos);           
            var regexLinhaDeMarcacaoDoTempo = new Regex("(?<tempoDeInicio>[0-9]{1,}:[0-9]{1,}:[0-9]{1,},[0-9]{1,}) --> (?<tempoDeFim>[0-9]{1,}:[0-9]{1,}:[0-9]{1,},[0-9]{1,})", RegexOptions.Compiled);            
            AjustaFormatacao(timeSpan, regexLinhaDeMarcacaoDoTempo, listaDeBlocos);                              
            EscreveNoArquivo(output, encoding, listaDeBlocos);            
            output.Close();
        }

        /// <summary>
        /// Faz a operação de escrita no arquivo que terá as marcações de tempo acrescidas em x milisegundos.
        /// </summary>
        private static void EscreveNoArquivo(Stream output, Encoding encoding, List<List<string>> listaDeBlocos)
        {
            foreach (var bloco in listaDeBlocos)
            {
                for (int i = 0; i < bloco.Count; i++)
                {
                    output.Write(encoding.GetBytes(bloco.ElementAt(i)), 0, encoding.GetBytes(bloco.ElementAt(i)).Length);
                    output.Write(encoding.GetBytes("\n"), 0, encoding.GetBytes("\n").Length);
                }
            }           
        }

        /// <summary>
        /// Entre o arquivo original e a saída desejada existem diferenças marcantes, como ponto final no lugar da vírgula para
        /// marcar os milisegundos, dentre outras diferenças de formatação. Esse método faz os ajustes necessários.
        /// </summary>
        private static void AjustaFormatacao(TimeSpan timeSpan, Regex regexLinhaDeMarcacaoDoTempo, List<List<string>> listaDeBlocos)
        {
            for (int k = 0; k < listaDeBlocos.Count; k++)
            {
                for (int l = 0; l < listaDeBlocos[k].Count; l++)
                {
                    var tempos = regexLinhaDeMarcacaoDoTempo.Match(listaDeBlocos.ElementAt(k).ElementAt(l));

                    if (tempos.Success && listaDeBlocos[k].Count > 2)
                    {
                        var inicio = tempos.Groups["tempoDeInicio"].Value;
                        var fim = tempos.Groups["tempoDeFim"].Value;
                        var inicioTime = ConverteParaTimeSpan(inicio) + timeSpan;
                        var fimTime = ConverteParaTimeSpan(fim) + timeSpan;
                        var regexParaAjustarMilissegundos = new Regex("[.][0-9]{7}", RegexOptions.Compiled);
                        string fimTimeAux, inicioTimeAux;

                        if (!regexParaAjustarMilissegundos.Match(fimTime.ToString()).Success)
                            fimTimeAux = fimTime.ToString() + ".000";
                        else
                            fimTimeAux = fimTime.ToString().Substring(0, fimTime.ToString().Length - 4);

                        if (!regexParaAjustarMilissegundos.Match(inicioTime.ToString()).Success)
                            inicioTimeAux = inicioTime.ToString() + ".000";
                        else
                            inicioTimeAux = inicioTime.ToString().Substring(0, inicioTime.ToString().Length - 4);

                        listaDeBlocos[k][l] = listaDeBlocos.ElementAt(k).ElementAt(l).Replace(inicio, inicioTimeAux).Replace(fim, fimTimeAux);
                    }
                }
            }
        }

        /// <summary>
        /// O método abaixo divide o conteúdo do arquivo original em blocos, por legenda, e monta uma lista com esses blocos.
        /// </summary>
        private static void CriaListaDeBlocosDeLegendas(string[] lines, out List<List<string>> listaDeBlocos)
        {            
            listaDeBlocos = new List<List<string>>();
            var regexNumeracaoDosBlocos = new Regex("^([0-9]{1,}$)", RegexOptions.Compiled);
            List<string> bloco = new List<string>();
            
            foreach (var line in lines)
            {
                if (regexNumeracaoDosBlocos.Match(line).Success && !string.IsNullOrEmpty(line))
                {
                    bloco = new List<string>();
                    listaDeBlocos.Add(bloco);
                }

                bloco.Add(line);
            }
        }

        /// <summary>
        /// Converte de string para TimeSpan os tempos indicados no arquivo. O Bloco Try-Catch é necessário para considerar
        /// as diferentes possibilidades de formatação das strings correspondentes.
        /// </summary>
        private static TimeSpan ConverteParaTimeSpan(string tempo)
        {
            try
            {
                var tempoEmTimeSpan = new TimeSpan(0, Convert.ToInt32(tempo.Substring(0, 2)), Convert.ToInt32(tempo.Substring(3, 2)), Convert.ToInt32(tempo.Substring(6, 2)), Convert.ToInt32(tempo.Substring(9, 3)));
                return tempoEmTimeSpan;
            }
            catch
            {
                var tempoEmTimeSpan = new TimeSpan(0, Convert.ToInt32(tempo.Substring(0, 2)), Convert.ToInt32(tempo.Substring(3, 2)), Convert.ToInt32(tempo.Substring(6, 1)), Convert.ToInt32(tempo.Substring(8, 3)));
                return tempoEmTimeSpan;
            }
        }              
    }
}