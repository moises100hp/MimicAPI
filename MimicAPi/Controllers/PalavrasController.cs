using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPi.Helpers;
using MimicAPi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimicAPi.Repositories.Contracts;
using AutoMapper;
using MimicAPi.Models.DTO;

namespace MimicAPi.Controllers
{
    [Route("api/palavras")]

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
        [Route("")]
        [HttpGet]
        public ActionResult ObterTodas([FromQuery]PalavraUrlQuery query)
        {
            var item = _repository.ObterPalavras(query);

            if (query.PagNumero > item.Paginacao.TotalPaginas)
            {
                return NotFound();
            }

            Response.Headers.Add("x-Pagination", JsonConvert.SerializeObject(item.Paginacao));

            return Ok(item.ToList());
        }
        //
        [Route("{id}")]
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);
            palavraDTO.Links = new List<LinkDTO>();
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
        [Route("{id}")]
        [HttpDelete]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _repository.Cadastrar(palavra);

            return Created($"/api/palavras/{palavra.Id}", palavra);
        }

        [Route("{id}")]
        [HttpPost("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
           var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            palavra.Id = id;
            _repository.Atualizar(palavra);

            return Ok();
        }

        [Route("{id}")]
        [HttpPost("{id}", Name = "ExcluirPalavra")]
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
