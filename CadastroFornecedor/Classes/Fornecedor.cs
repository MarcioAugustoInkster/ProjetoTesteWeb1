using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CadastroFornecedor.Classes
{
    public class Fornecedor
    {
        public Empresa Empresa { get; set; }
        public string Nome { get; set; }
        public string Rg { get; set; }
        public string CpfCnpj { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Telefone1 { get; set; }
        public string Telefone2 { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
