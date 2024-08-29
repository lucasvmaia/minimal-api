using minimal_api.Domain.Entities;
namespace Test.Domain.Entities
{
    [TestClass]
    public class AdministradorTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            var adm = new Administrador
            {
                Id = 1,
                Email = "teste@teste.com.br",
                Senha = "teste",
                Perfil = "Adm"
            };
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("teste@teste.com.br", adm.Email);
            Assert.AreEqual("teste", adm.Senha);
            Assert.AreEqual("Adm", adm.Perfil);
    
        }
    }
}