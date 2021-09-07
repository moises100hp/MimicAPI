﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPi.Helpers;
using MimicAPi.V1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimicAPi.V1.Repositories.Contracts;
using AutoMapper;
using MimicAPi.V1.Models.DTO;

namespace MimicAPi.V1.Controllers
{
    // /api/v1.0/palavras

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.1")]

    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        
        //APP
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult ObterTodas([FromQuery]PalavraUrlQuery query)
        {
            var item = _repository.ObterPalavras(query);


            if (item.Results.Count == 0)
                return NotFound();
            PaginationList<PalavraDTO> lista = CriarLinksListPalavraDTO(query, item);

            return Ok(lista);
        }

        private PaginationList<PalavraDTO> CriarLinksListPalavraDTO(PalavraUrlQuery query, PaginationList<Palavra> item)
        {
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            foreach (var palavra in lista.Results)
            {
                palavra.Links = new List<LinkDTO>();
                palavra.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavra.Id }), "GET"));
            }

            lista.Links.Add(new LinkDTO("self", Url.Link("ObterTodas", query), "GET"));

            if (item.Paginacao != null)
            {
                Response.Headers.Add("x-Pagination", JsonConvert.SerializeObject(item.Paginacao));

                if (query.PagNumero + 1 <= item.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }
                if (query.PagNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("prev", Url.Link("ObterTodas", queryString), "GET"));
                }
            }

            return lista;
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("{id}")]
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);
            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new  { id = palavraDTO.Id}), "GET")
                );
            palavraDTO.Links.Add(
               new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id}), "PUT")
               );
            palavraDTO.Links.Add(
               new LinkDTO("delete", Url.Link("ExcluirPalavra", new { id = palavraDTO.Id}), "DELETE")
               );

            return Ok(palavraDTO);
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;
            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO =  _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
              new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
              );

            return Created($"/api/palavras/{palavra.Id}", palavra);
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("{id}")]
        [HttpPut("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
           var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Id = id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;
            _repository.Atualizar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
             new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
             );



            return Ok();
        }

        [MapToApiVersion("1.1")]
        [Route("{id}")]
        [HttpDelete("{id}", Name = "ExcluirPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);

            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);

            return NoContent();
        }



    }
}
