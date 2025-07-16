using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pitpo_lab4_enc; 

namespace pitpo_lab4_enc_integration_tests
{
    [TestClass]
    public class BigBangIntegrationTests
    {
        private IArticleRepository _repository;
        private ArticleFacade _facade;

        [TestInitialize]
        public void Setup()
        {
            _repository = new PostgresArticleRepository();
            _facade = new ArticleFacade(_repository);
        }

        [TestMethod]
        public void Test_GetFilteredArticles_AllArticlesReturnedIfNoKeywordProvided()
        {
            int minViews = 10;
            string keyword = "";

            List<Article> articles = _facade.GetFilteredArticles(minViews, keyword);

            Assert.IsNotNull(articles, "Список статей не должен быть null.");
            Assert.IsTrue(articles.Count > 0, "Должны быть найдены статьи, удовлетворяющие условию.");
            foreach (var article in articles)
            {
                Assert.IsTrue(article.Views >= minViews,
                    "Все статьи должны иметь количество просмотров не менее заданного.");
            }
        }

        [TestMethod]
        public void Test_GetFilteredArticles_KeywordFilterWorks()
        {
            int minViews = 0;
            string keyword = "Шатт";

            List<Article> articles = _facade.GetFilteredArticles(minViews, keyword);

            Assert.IsNotNull(articles, "Список статей не должен быть null.");
            Assert.IsTrue(articles.Count > 0, "Должны быть найдены статьи с указанным ключевым словом.");

            foreach (var article in articles)
            {
                bool containsKeyword = article.Title.ToLower().Contains(keyword.ToLower()) ||
                    (article.Tags != null && article.Tags.Exists(tag => tag.ToLower().Contains(keyword.ToLower())));
                Assert.IsTrue(containsKeyword,
                    $"Статья \"{article.Title}\" не содержит ключевое слово \"{keyword}\" в заголовке или тегах.");
            }
        }

        [TestMethod]
        public void Test_GetFilteredArticles_NoArticlesFound()
        {
            int minViews = int.MaxValue;
            string keyword = "nonexistent";

            List<Article> articles = _facade.GetFilteredArticles(minViews, keyword);

            Assert.IsNotNull(articles, "Список статей не должен быть null.");
            Assert.AreEqual(0, articles.Count,
                "Должен быть возвращён пустой список, если ни одна статья не удовлетворяет условиям.");
        }
    }
}
