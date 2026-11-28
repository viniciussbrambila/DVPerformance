# 🏋️‍♂️ DV Performance — Sistema de Gestão e Prescrição de Treinos

![Status do Projeto](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)
![React](https://img.shields.io/badge/Front--End-React%20%2B%20MUI-61DAFB?logo=react)
![Back-End](https://img.shields.io/badge/Back--End-C%23%20%28.NET%2010%29-512BD4?logo=dotnet)
![Database](https://img.shields.io/badge/Banco%20de%20Dados-PostgreSQL%20%28Supabase%29-3ECF8E?logo=supabase)
![License](https://img.shields.io/badge/Projeto-Acad%C3%AAmico%20%2F%20TCC-blue)

## 📌 Sobre o Projeto

O **DV Performance** é uma plataforma web voltada exclusivamente para o **Personal Trainer**, com foco na **gestão de alunos e na automação inteligente da montagem de fichas de treino**.

Desenvolvido como projeto de Estágio Supervisionado e Trabalho de Conclusão de Curso (TCC) em **Análise e Desenvolvimento de Sistemas (UMFG)**, o software centraliza o trabalho do Personal em um ambiente próprio e padroniza a prescrição personalizada para cada aluno.

### Modelo de Acesso e Uso
- **Personal Trainer como usuário único:** cada conta cadastrada pertence a um Personal Trainer, que administra de forma isolada os seus próprios alunos, catálogos personalizados e planos de treino (não há separação entre perfis de *Admin* e *Personal*).
- **Foco no trabalho do Personal:** a aplicação foi desenhada para o fluxo de planejamento, prescrição e acompanhamento conduzido pelo próprio Personal Trainer.

---

## 🎯 Objetivos do Sistema

- **Centralização de Cadastros:** Gerenciar de forma integrada os registros de alunos, exercícios, máquinas e fichas de treino vinculadas ao Personal autenticado.
- **Automação de Treinos:** Gerar prescrições personalizadas aplicando regras fisiológicas e anatômicas baseadas no perfil do aluno (sexo, idade calculada pela data de nascimento, objetivo, frequência semanal e nível de experiência).
- **Agilidade e Padronização:** Reduzir o tempo gasto na montagem manual de fichas e facilitar a edição, exportação e impressão dos planos de treino.

---

## ⚙️ Funcionalidades Implementadas

- [x] **Autenticação, Segurança e Conta do Personal:**
  - Cadastro e login com senha em texto simples e autenticação via token **JWT (HMAC-SHA256)**.
- [x] **Dashboard do Personal:** Visão geral com contagens de alunos cadastrados e planos gerados, além de atalhos de acesso rápido e alternância de tema claro/escuro persistida.
- [x] **Gestão de Alunos:** Cadastro completo com data de nascimento, peso, altura, experiência, objetivo, frequência semanal, filtros por período, busca e relatório individual de evolução/características.
- [x] **Catálogo de Exercícios e Máquinas:** Provisionamento automático de catálogo inicial por conta, criação/edição personalizada, definição de foco muscular e nível mínimo exigido.
- [x] **Gerador Inteligente de Fichas:** Seleção de divisão de treino (Full Body, Upper/Lower, PPL, Bro Split etc.), distribuição por prioridade anatômica e focos musculares, dicas por objetivo sem repetição cíclica e prescrição de séries/repetições/descanso/aeróbico.
- [x] **Edição e Exportação de Planos:** Edição manual de exercícios/máquinas, reordenação por dia, histórico de planos salvos com filtros e visualização preparada para impressão/exportação.

---

## 🚀 Tecnologias Utilizadas

### **Front-End**
- **[React.js](https://react.dev/) (via Vite):** Construção da interface SPA (*Single Page Application*) de forma modular e reativa.
- **[JavaScript (ES6+)](https://developer.mozilla.org/pt-BR/docs/Web/JavaScript):** Lógica de front-end, manipulação de estados e testes unitários com `node --test`.
- **[Material UI (MUI)](https://mui.com/):** Componentes visuais seguindo o Material Design, com temas claro e escuro customizados via `ThemeProvider` e estilização com `sx` e `styled()`.
- **[React Router DOM](https://reactrouter.com/):** Gerenciamento de rotas da aplicação.

### **Back-End & Banco de Dados**
- **C# (.NET 10 — Minimal APIs):** API REST responsável pela autenticação, isolamento multi-tenant por Personal e motor de geração e validação de treinos.
- **PostgreSQL no [Supabase](https://supabase.com/) (via Entity Framework Core):** Persistência em nuvem com *migrations* versionadas, restrições de integridade e isolamento por conta.

### **Ferramentas, Modelagem & Gestão**
- **IDEs:** Visual Studio Code / JetBrains Rider / Visual Studio
- **Modelagem de Sistema:** Astah UML (Diagramas de Caso de Uso, Classe, Sequência, Atividades e Máquina de Estados)
- **Versionamento:** Git & GitHub (utilizando padrão de *Commits Semânticos*)
