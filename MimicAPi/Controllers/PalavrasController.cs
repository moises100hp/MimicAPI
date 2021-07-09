using Microsoft.AspNetCore.Mvc;
using MimicAPi.Database;
using MimicAPi.Helpers;
using MimicAPi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPi.Controllers
{
    [Route("api/palavras")]

    public class PalavrasController : ControllerBase
    {
        private readonly MimicContext _banco;

        public PalavrasController(MimicContext banco)
        {
            _banco = banco;
        }

        
        //APP
        [Route("")]
        [HttpGet]
        public ActionResult ObterPalavras([FromQuery]PalavraUrlQuery query)
        {
            var item = _banco.Palavras.AsQueryable();

            if (query.Data.HasValue)
            {
                item = item.Where(a => a.Criado > query.Data.Value || a.Atualizado > query.Data.Value);
            }

            if (query.PagNumero.HasValue)
            {
                //paginação
                var quantidadeTotalRegistros = item.Count();
                item = item.Skip((query.PagNumero.Value - 1) * query.PagRegistro.Value).Take(query.PagRegistro.Value);

                var paginacao = new Paginacao();

                paginacao.NumeroPagina = query.PagNumero.Value;
                paginacao.RegistroPorPagina = query.PagRegistro.Value;
                paginacao.TotalRegistros = quantidadeTotalRegistros;
                paginacao.TotalPaginas = (int) Math.Ceiling((double) quantidadeTotalRegistros / query.PagRegistro.Value);

                Response.Headers.Add("x-Pagination", JsonConvert.SerializeObject(paginacao));

                if(query.PagNumero > paginacao.TotalPaginas)
                {
                    return NotFound();
                }
            }
            return Ok(item);
        }
        //
        [Route("{id}")]
        [HttpGet]
        public ActionResult ObtertPalavra(int id)
        {
            return Ok(_banco.Palavras.Find(id));
        }
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _banco.Palavras.Add(palavra);
            _banco.SaveChanges();
            return Ok();
        }

        [Route("{id}")]
        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
            palavra.Id = id;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();

            return Ok();
        }

        [Route("{id}")]
        [HttpDelete]
        public ActionResult Deletar(int id)
        {
            var palavra = _banco.Palavras.Find(id);
            palavra.Ativo = false;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
            
            return Ok();
        }


    }
}
