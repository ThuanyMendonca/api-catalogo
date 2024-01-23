using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork context, IMapper mapper)
        {
            _uof = context;
            _mapper = mapper;
        }

        [HttpGet("menorpreco")]
        public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosPreco()
        {
            List<Produto> produtos = _uof.ProdutoRepository.GetProdutosPorPreco().ToList();
            List<ProdutoDTO> produtosDto = _mapper.Map<List<ProdutoDTO>>(produtos);

            return produtosDto;
        }

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult<IEnumerable<ProdutoDTO>> Get([FromQuery] ProdutosParameters produtosParameters)
        {
            var produtos = _uof.ProdutoRepository.GetProdutos(produtosParameters).ToList();
            if(produtos is null)
            {
                return NotFound();
            }

            var produtosDto = _mapper.Map<List<ProdutoDTO>>(produtos);
            return produtosDto;
        }

        [HttpGet("{id:int}", Name = "ObterProduto")]
        public ActionResult<ProdutoDTO> Get(int id)
        {
            //throw new Exception("Exception ao retornar produto pelo id");
            
            Produto produto = _uof.ProdutoRepository.GetById(p => p.Id == id);
            if (produto is null)
            {
                return NotFound();
            }

            ProdutoDTO produtosDto = _mapper.Map<ProdutoDTO>(produto);

            return produtosDto;
        }

        [HttpPost]
        public ActionResult Post(ProdutoDTO produtoDto)
        {
            if (produtoDto is null)
            {
                return BadRequest();
            }

            // transforma os dados da DTO em Produto Model
            var produto = _mapper.Map<Produto>(produtoDto);

            _uof.ProdutoRepository.Add(produto); // inclui apenas no contexto em memória
            _uof.Commit(); // persiste os dados no banco de dados

            /*
             * retorna 201 created
             * aciona a rota ObterProduto com o id do produto
             * retorna um id do produto criado, adiciona o id no cabeçalho location 
             * e especificar a uri para obter o produto criado
            */

            // transforma o Produto Model em ProdutoDTO
            var produtoDTO = _mapper.Map<ProdutoDTO>(produto);
            return new CreatedAtRouteResult("ObterProduto", new { id = produto.Id }, produtoDTO);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, ProdutoDTO produtoDto)
        {
            if (id != produtoDto.Id)
            {
                return BadRequest();
            }

            //_uof.Entry(produto).State = EntityState.Modified;
            //_uof.SaveChanges();

            Produto produto = _mapper.Map<Produto>(produtoDto);

            _uof.ProdutoRepository.Update(produto);
            _uof.Commit();
            return Ok();
        }       

        [HttpDelete("{id:int}")]
        public ActionResult<ProdutoDTO> Delete(int id) 
        {
            Produto produto = _uof.ProdutoRepository.GetById(p => p.Id == id);
            /* 
             * alternativa é o Find que busca primeiro na memória e caso não encontre, ele vai ao banco de dados
             * porém o parâmetro deve ser primary key
            */
            //var produto = _uof.Produtos.Find(id);
            
            if(produto is null)
            {
                return NotFound("Produto não localizado");
            }

            _uof.ProdutoRepository.Delete(produto);
            _uof.Commit();

            ProdutoDTO produtoDto = _mapper.Map<ProdutoDTO>(produto);

            return Ok(produtoDto);
        }

    }
}
