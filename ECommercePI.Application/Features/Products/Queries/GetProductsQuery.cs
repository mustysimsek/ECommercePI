using ECommercePI.Domain.Entities;
using MediatR;

namespace ECommercePI.Application.Features.Products.Queries;

public record GetProductsQuery() : IRequest<List<Product>>;