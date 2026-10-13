# Requisitos da atividade — Navegando pelo mundo DevOps

Documento de consulta com o enunciado extraído dos prints da atividade avaliativa. Serve para conferir o que precisa ser entregue; não substitui a entrega na plataforma.

---

## Atenção! Atividade avaliativa

- Verifique se o **arquivo do upload está correto**. Não é possível enviar outro arquivo após o fechamento da entrega na plataforma ou após a correção do professor.
- Não deixe para realizar a entrega da atividade **nos últimos minutos do prazo**. Você pode ter algum problema e perder a entrega. As entregas são realizadas apenas pela plataforma.
- **Não disponibilize a resposta** da sua atividade em grupos de WhatsApp, Discord ou Microsoft Teams, pois pode gerar plágio e zerar a atividade para todos.
- Você tem um período máximo de **15 dias** após a publicação da nota para solicitar a revisão da correção.

---

## Contexto

Este desafio serve como uma junção prática dos conceitos e habilidades adquiridas na fase **"Navegando pelo mundo DevOps"**. Serão aplicadas diretamente as técnicas de DevOps em um contexto real, utilizando como base os projetos desenvolvidos em **Java Spring ou C#** nos desafios anteriores.

Para este exercício, cada aluno ou grupo de alunos deve escolher entre o projeto Java Spring ou C# desenvolvido anteriormente com o tema de **ESG** e integrar práticas completas de DevOps para automatizar todo o ciclo de vida da aplicação. A aplicação escolhida deverá ser adaptada e melhorada para simular um ambiente de produção real, onde a agilidade e a robustez são cruciais.

---

## Objetivos da atividade

- Criar um **pipeline funcional de CI/CD** com etapas automatizadas de **build**, **testes** e **deploy**.
- Containerizar a aplicação.
- Orquestrar a infraestrutura (**Docker Compose** ou **Kubernetes**).
- Entregar o projeto empacotado com **documentação técnica** e **evidências**.

---

## Requisitos da atividade

### 1. Pipeline de CI/CD

Implementar um pipeline de integração contínua e *deployment* contínuo utilizando:

- **Azure DevOps**, **GitHub Actions**, **Jenkins**, **CircleCI** ou outra ferramenta DevOps.

O pipeline deve conter:

- Etapa de **build** automática.
- **Testes automatizados** (mínimo: execução de testes existentes).
- **Deploy automatizado** em dois ambientes: **staging** e **produção**.

### 2. Containerização e orquestração

- Criar um **Dockerfile** funcional para a aplicação.
- Criar um **docker-compose.yml** ou configuração **Kubernetes** para orquestrar os serviços (ex.: aplicação + banco).
- Utilizar **volumes**, **variáveis de ambiente** e **redes** para compor o ambiente.

---

## Artefatos da entrega

### 1) Código-fonte (.zip)

Inclua todo o projeto com a seguinte estrutura mínima:

```text
seu-projeto/
├── Dockerfile
├── docker-compose.yml (ou pasta Kubernetes/)
├── src/
├── README.md
└── .github/workflows/ (caso GitHub Actions)
```

O arquivo `.ZIP` deve conter:

- Código-fonte completo.
- Arquivos de configuração de CI/CD.
- Dockerfile e orquestrador (Compose ou Kubernetes).
- Scripts, configurações de ambiente, `.env.example`.
- Logs ou prints opcionais.

### 2) README.md

Inclua um `README.md` com as seguintes seções:

```markdown
# Projeto - Cidades ESGInteligentes

## Como executar localmente com Docker

Descreva os passos para subir a aplicação.

## Pipeline CI/CD

Explique as ferramentas utilizadas, as etapas do pipeline e seu funcionamento.

## Containerização

Mostre o conteúdo do Dockerfile e as estratégias adotadas.

## Prints do funcionamento

Inclua evidências (prints ou links) de execução, deploy e funcionamento em staging e produção.

## Tecnologias utilizadas

Liste as stacks, frameworks e ferramentas usadas.
```

### 3) Documentação (.PDF ou .PPT)

Crie um documento **PDF** ou **PPT** com os seguintes tópicos:

- Título do projeto + nome dos integrantes.
- Descrição do **pipeline**: ferramenta usada, etapas e lógica.
- **Docker**: arquitetura, comandos usados, imagem criada.
- Prints do pipeline rodando (**build**, **testes**, **deploy**).
- Prints dos ambientes **staging** e **produção** funcionando.
- Desafios encontrados e como foram resolvidos.

### 4) Checklist de entrega (obrigatório)

Preencha e anexe este checklist no **final do seu PDF ou README**.

| Item | OK |
| --- | --- |
| Projeto compactado em .ZIP com estrutura organizada | |
| Dockerfile funcional | |
| docker-compose.yml ou arquivos Kubernetes | |
| Pipeline com etapas de build, teste e deploy | |
| README.md com instruções e prints | |
| Documentação técnica com evidências (PDF ou PPT) | |
| Deploy realizado nos ambientes staging e produção | |
