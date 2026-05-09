using System;
using System.Collections.Generic;
using System.Linq;

namespace Microservicio.Vuelos.DataAccess.Common
{
    /// <summary>
    /// Clase genérica para encapsular resultados paginados.
    /// Usada por los QueryRepositories al retornar listas filtradas.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; private set; }

        public int TotalRegistros { get; private set; }

        public int PaginaActual { get; private set; }

        public int TamanoPagina { get; private set; }

        public int TotalPaginas =>
            TamanoPagina <= 0
                ? 0
                : (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);

        public bool TienePaginaAnterior => PaginaActual > 1;

        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        public PagedResult(
            IEnumerable<T> items,
            int totalRegistros,
            int paginaActual,
            int tamanoPagina)
        {
            if (paginaActual <= 0)
                throw new ArgumentException("La página actual debe ser mayor a 0");

            if (tamanoPagina <= 0)
                throw new ArgumentException("El tamaño de página debe ser mayor a 0");

            Items = items.ToList().AsReadOnly();
            TotalRegistros = totalRegistros;
            PaginaActual = paginaActual;
            TamanoPagina = tamanoPagina;
        }

        public static PagedResult<T> Crear(
            IEnumerable<T> items,
            int totalRegistros,
            int paginaActual,
            int tamanoPagina)
        {
            return new PagedResult<T>(items, totalRegistros, paginaActual, tamanoPagina);
        }
    }
}