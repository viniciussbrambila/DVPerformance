# 🏋️‍♂️ DV Performance — Sistema de Gestão e Prescrição de Treinos

![Status do Projeto](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)
![React](https://img.shields.io/badge/Front--End-React%20%2B%20MUI-61DAFB?logo=react)
![Back-End](https://img.shields.io/badge/Back--End-C%23%20%28Planejado%29-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/Projeto-Acad%C3%AAmico%20%2F%20TCC-blue)

## 📌 Sobre o Projeto

O **DV Performance** é uma plataforma web voltada para a gestão de estúdios de treinamento e academias, com foco principal na **automação e montagem inteligente de fichas de treino**. 

Desenvolvido como projeto de Estágio Supervisionado e Trabalho de Conclusão de Curso (TCC) em **Análise e Desenvolvimento de Sistemas (UMFG)**, o software busca otimizar o tempo dos instrutores e padronizar a qualidade do atendimento aos alunos.

---

## 🎯 Objetivos do Sistema

- **Centralização de Cadastros:** Gerenciar de forma integrada os registros de alunos, instrutores, exercícios e equipamentos/máquinas disponíveis no espaço.
- **Automação de Treinos:** Gerar prescrições de treinos personalizadas aplicando regras de negócio baseadas no perfil do aluno (sexo, faixa etária, objetivo e nível de experiência).
- **Interface Ágil e Responsiva:** Proporcionar uma experiência de uso fluida tanto no desktop da recepção/coordenação quanto em tablets ou celulares utilizados pelos instrutores no salão.

---

## ⚙️ Funcionalidades Principais

- [x] Estruturação base do Front-End e Tema Customizado (MUI)
- [ ] **Autenticação e Controle de Acesso:** Login para administradores e instrutores.
- [ ] **Gestão de Alunos:** Cadastro completo, histórico, anamnese e nível de treinamento.
- [ ] **Catálogo de Exercícios e Máquinas:** Mapeamento de grupos musculares, equipamentos necessários e variáveis de carga/repetição.
- [ ] **Gerador Inteligente de Fichas:** Montagem automatizada e edição manual de rotinas de treino.
- [ ] **Dashboard Administrativo:** Visão geral de alunos ativos e métricas do estúdio.

---

## 🚀 Tecnologias Utilizadas

### **Front-End**
- **[React.js](https://react.dev/) (via Vite):** Biblioteca principal para construção da interface de usuário de forma rápida e modular.
- **[JavaScript (ES6+)](https://developer.mozilla.org/pt-BR/docs/Web/JavaScript):** Lógica de front-end e manipulação de estados.
- **[Material UI (MUI)](https://mui.com/):** Biblioteca de componentes visuais seguindo o Material Design, utilizando estilização via `sx` prop e *ThemeProvider* para consistência visual.
- **[React Router DOM](https://reactrouter.com/):** Gerenciamento de rotas e navegação SPA (*Single Page Application*).

### **Back-End & Banco de Dados *(Em estruturação)***
- **C# (.NET):** Construção da API e processamento das regras de negócio de prescrição de treinos.
- **Banco de Dados Relacional (PostgreSQL / SQL):** Persistência segura dos dados de alunos, fichas e equipamentos.

### **Ferramentas, Modelagem & Gestão**
- **IDE:** Visual Studio Code / JetBrains Rider / Visual Studio
- **Modelagem de Sistema:** Astah UML (Diagramas de Caso de Uso, Classe, Sequência, Atividades e Máquina de Estados)
- **Versionamento:** Git & GitHub (utilizando padrão de *Commits Semânticos*)
