/******** ATENÇÃO ********/
/*
 * ESTE EXEMPLO FOI CRIADO USANDO A VERSÃO BETA 1 DO ODP.NET DA ORACLE PARA .NET CORE 
 * NESTA VERSÃO DO DRIVER EXISTE UM BUG ONDE A DLL DEVE SER REFERENCIADA NO MESMO PROJETO ONDE ESTÃO SENDO CHAMADOS OS 
 * MÉTODOS QUE ACESSAM OS DADOS, SENÃO CAUSA UMA EXCEPTION.
 * QUANDO A ORACLE ATUALIZAR O DRIVER E SOLUCIONAR O BUG, A DEPENDENCIA DO PROJETO DEVE SER ELIMINADA E MANTIDA SÓ NA
 * CLASS LIBRARY "ORACLEDAL" DA SOLUÇÃO. O DRIVER MAIS RECENTE PODE SER ENCONTRADO AQUI:
 * http://www.oracle.com/technetwork/topics/dotnet/downloads/odpnetcorebeta-4077982.html
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace OracleDAL
{
    class Program
    {
        static void Main(string[] args)
        {
            OracleClient.ConnectionString = "User Id=usuario;Password=senha;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.0.1)(PORT=1521))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME=SERV)))";
            string sql = "select id, nome from teste";
            List<Teste> temps = OracleClient.QueryForList<Teste>(sql).ToList();
            foreach (var item in temps)
            {
                Console.WriteLine(item.Id + "   " + item.Nome);
            }
            Console.ReadLine();
        }
    }

    public class Teste
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}
