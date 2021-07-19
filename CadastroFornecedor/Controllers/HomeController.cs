using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CadastroFornecedor.Models;
using CadastroFornecedor.Classes;

namespace CadastroFornecedor.Controllers
{
    public class HomeController : Controller
    {
        private static List<Empresa> listaEmpresas = new List<Empresa>();
        private static List<Fornecedor> listaFornecedores = new List<Fornecedor>();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Empresa()
        {
            return View();
        }

        public IActionResult Fornecedor()
        {
            FornecedorModel forn = new FornecedorModel()
            {
                Empresas = listaEmpresas,
                DataNascimento = new DateTime()
            };

            return View(forn);
        }

        [HttpGet]
        public IActionResult EmpresaCadastro()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FornecedorCadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EmpresaCadastro([FromForm] EmpresaModel model)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    Empresa empresa = new Empresa()
                    {
                        NomeFantasia = model.NomeFantasia,
                        Cnpj = model.Cnpj,
                        UF = model.UF
                    };

                    listaEmpresas.Add(empresa);
                }
            }

            return RedirectToAction("EmpresaLista", "Home");
        }

        [HttpPost]
        public IActionResult FornecedorCadastro([FromForm] FornecedorModel model)
        {
            if (model != null)
            {
                if (listaEmpresas.Count > 0)
                {
                    Empresa empresa = new Empresa();
                    bool restritaEstado = false, pessoaFisica = false, menorDeIdade = false;

                    foreach (Empresa emp in listaEmpresas)
                    {
                        if (emp.NomeFantasia.ToLower().Equals(model.Empresa.ToLower()))
                        {
                            empresa.NomeFantasia = emp.NomeFantasia;
                            empresa.Cnpj = emp.Cnpj;
                            empresa.UF = emp.UF;

                            if (empresa.UF.ToUpper().Equals("PR"))
                                restritaEstado = true;

                            break;
                        }
                    }

                    if (model.CpfCnpj.Contains(".") && model.CpfCnpj.Contains("-") && model.CpfCnpj.Length == 14)
                        pessoaFisica = true;

                    if (ValidaMenorDeIdade(model.DataNascimento) < 18)
                        menorDeIdade = true;

                    if (restritaEstado && pessoaFisica && menorDeIdade)
                    {
                        return RedirectToAction("Mensagem", "Home", new { requestCode = 403, responseMsg = "Se a empresa é do Paraná, o fornecedor com CPF deve ser maior de idade." });
                    }
                    else
                    {
                        bool validaRg = false;

                        if (pessoaFisica)
                        {
                            if (string.IsNullOrEmpty(model.Rg) || (model.Rg.Length != 9 && !model.Rg.Contains(".")))
                                validaRg = true;

                            if (model.DataNascimento.ToString().Equals("01/01/0001 00:00:00") && validaRg)
                                return RedirectToAction("Mensagem", "Home", new { requestCode = 404, responseMsg = "Se o fornecedor é Pessoa Física, os campos [RG] e [Data de nascimento] devem ser preenchidos." });
                        }

                        Fornecedor fornecedor = new Fornecedor()
                        {
                            Empresa = empresa,
                            Nome = model.Nome,
                            Rg = model.Rg,
                            CpfCnpj = model.CpfCnpj,
                            DataNascimento = model.DataNascimento,
                            Telefone1 = model.Telefone1,
                            Telefone2 = model.Telefone2,
                            DataCadastro = DateTime.Now
                        };

                        listaFornecedores.Add(fornecedor);
                        return RedirectToAction("FornecedorLista", "Home");
                    }
                }
            }
            return RedirectToAction("Fornecedor", "Home");
        }

        public IActionResult EmpresaLista()
        {
            List<EmpresaModel> listagem = new List<EmpresaModel>();

            if (listaEmpresas.Count > 0)
            {
                foreach (Empresa empresa in listaEmpresas)
                {
                    EmpresaModel model = new EmpresaModel()
                    {
                        NomeFantasia = empresa.NomeFantasia,
                        Cnpj = empresa.Cnpj,
                        UF = empresa.UF
                    };

                    listagem.Add(model);
                }
            }
            return View(listagem);
        }

        [HttpGet]
        public IActionResult FornecedorLista(string filtro)
        {
            List<FornecedorModel> listagem = new List<FornecedorModel>();

            if (listaFornecedores.Count > 0)
            {
                if (!string.IsNullOrEmpty(filtro))
                {
                    foreach (Fornecedor fornecedor in listaFornecedores)
                    {
                        if (fornecedor.Nome.ToLower().Contains(filtro.ToLower()) ||
                            fornecedor.CpfCnpj.Contains(filtro) ||
                            fornecedor.DataCadastro.ToString().Contains(filtro))
                        {
                            FornecedorModel fm = ListaResultados(fornecedor);
                            listagem.Add(fm);
                        }
                    }
                }
                else
                {
                    foreach (Fornecedor fornecedor in listaFornecedores)
                    {
                        FornecedorModel fm = ListaResultados(fornecedor);
                        listagem.Add(fm);
                    }
                }
            }
            return View(listagem);
        }

        public FornecedorModel ListaResultados(Fornecedor fornecedor)
        {
            FornecedorModel model = new FornecedorModel()
            {
                Empresa = fornecedor.Empresa.NomeFantasia,
                Nome = fornecedor.Nome,
                CpfCnpj = fornecedor.CpfCnpj
            };

            if (!string.IsNullOrEmpty(fornecedor.Telefone1))
            {
                model.Telefone1 = fornecedor.Telefone1;
            }

            if (!string.IsNullOrEmpty(fornecedor.Telefone2))
            {
                model.Telefone2 = fornecedor.Telefone2;
            }
            return model;
        }

        [HttpPost]
        public IActionResult Pesquisa(string campoFiltro)
        {
            return RedirectToAction("FornecedorLista", "Home", new { filtro = campoFiltro });
        }

        public IActionResult Mensagem(int requestCode, string responseMsg)
        {
            var mensagem = new MensagemInfo()
            {
                Codigo = requestCode,
                Mensagem = responseMsg
            };

            return View(mensagem);
        }

        private int ValidaMenorDeIdade(DateTime dataNascimento)
        {
            int idade = DateTime.Now.Year - dataNascimento.Year;

            if (DateTime.Now.DayOfYear < dataNascimento.DayOfYear)
                idade = idade - 1;

            return idade;
        }
    }
}
