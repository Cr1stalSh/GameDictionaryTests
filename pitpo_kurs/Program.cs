using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql; 

namespace pitpo_lab4_enc
{
    public class ArticleFacade
    {
        private readonly IArticleRepository _repository;

        public ArticleFacade(IArticleRepository repository)
        {
            _repository = repository;
        }

        public List<Article> GetFilteredArticles(int minViews, string keyword)
        {
            List<Article> articles = _repository.GetAllArticles();
            if (articles == null)
            {
                Console.WriteLine("Не удалось загрузить статьи из базы данных.");
                return new List<Article>();
            }

            string lowerKeyword = string.IsNullOrWhiteSpace(keyword) ? "" : keyword.Trim().ToLower();

            List<Article> filteredArticles = new List<Article>();

            foreach (var article in articles)
            {
                if (article.Views < minViews)
                    continue;

                if (string.IsNullOrEmpty(lowerKeyword))
                {
                    filteredArticles.Add(article);
                }
                else
                {
                    bool matchesTitle = article.Title.ToLower().Contains(lowerKeyword);
                    bool matchesTags = article.Tags != null &&
                        article.Tags.Any(tag => tag.ToLower().Contains(lowerKeyword));

                    if (matchesTitle || matchesTags)
                        filteredArticles.Add(article);
                }
            }

            return filteredArticles;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            IArticleRepository repository = new PostgresArticleRepository();
            ArticleFacade facade = new ArticleFacade(repository);

            Console.Write("Введите минимальное количество просмотров: ");
            if (!int.TryParse(Console.ReadLine(), out int minViews) || minViews < 0)
            {
                Console.WriteLine("Ошибка: введите корректное число просмотров.");
                return;
            }

            Console.Write("Введите ключевое слово для поиска (оставьте пустым для вывода всех): ");
            string keyword = Console.ReadLine();

            List<Article> filteredArticles = facade.GetFilteredArticles(minViews, keyword);

            if (filteredArticles.Count == 0)
            {
                Console.WriteLine("Нет статей, удовлетворяющих условиям.");
            }
            else
            {
                Console.WriteLine("Найденные статьи:");
                foreach (var article in filteredArticles)
                {
                    Console.WriteLine($"- {article.Title} ({article.Views} просмотров)");
                }
            }
        }
    }

    public interface IArticleRepository
    {
        List<Article> GetAllArticles();
    }

    public class PostgresArticleRepository : IArticleRepository
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=postgres";

        public List<Article> GetAllArticles()
        {
            List<Article> articles = new List<Article>();

            try
            {
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT title, description, views, tags FROM articles";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader.GetString(0);
                            string description = reader.GetString(1);
                            int views = reader.GetInt32(2);
                            List<string> tags = new List<string>();

                            if (!reader.IsDBNull(3))
                            {
                                string tagsStr = reader.GetString(3);
                                tags.AddRange(tagsStr.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                            }

                            articles.Add(new Article(title, description, views, tags));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при подключении к базе данных: " + ex.Message);
                return null;
            }
            return articles;
        }
    }

    public class Article
    {
        public string Title { get; }
        public string Description { get; }
        public int Views { get; }
        public List<string> Tags { get; }

        public Article(string title, string description, int views, List<string> tags)
        {
            Title = title;
            Description = description;
            Views = views;
            Tags = tags;
        }
    }
}
