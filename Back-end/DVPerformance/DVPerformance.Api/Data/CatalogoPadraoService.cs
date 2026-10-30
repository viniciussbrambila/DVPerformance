using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Data;

/// <summary>
/// Catálogo oficial pré-cadastrado do DV Performance para Academias Brasileiras.
/// Provisiona cópias exclusivas (UsuarioId = personal.Id) para contas antigas e novas,
/// garantindo que cada Personal possa remover, editar ou criar itens sem afetar outras contas.
/// </summary>
public static class CatalogoPadraoService
{
    public sealed record ItemCatalogoPadrao(int GrupoMuscularId, string Nome, string Descricao);

    // IDs fixos da tabela mestre grupos_musculares (definidos em AppDbContext):
    // 1 = Peito | 2 = Costas | 3 = Quadríceps | 4 = Posterior de Coxa | 5 = Glúteos
    // 6 = Ombros | 7 = Trapézio | 8 = Adutores / Abdutores | 9 = Bíceps | 10 = Tríceps
    // 11 = Panturrilha | 12 = Antebraço | 13 = Abdômen | 14 = Lombar

    public static readonly IReadOnlyList<ItemCatalogoPadrao> ExerciciosPadrao =
    [
        // ==================== 1. PEITO (9 Exercícios Livres / Peso Corporal) ====================
        new(1, "Supino Reto com Barra", "Empurrão horizontal clássico no banco reto com barra longa; base multiarticular para força e volume global do peitoral maior."),
        new(1, "Supino Reto com Halteres", "Execução no banco reto com halteres independentes, permitindo maior amplitude de alongamento e estabilização escapular."),
        new(1, "Supino Inclinado com Barra", "Supino em banco a 30°–45° com barra, direcionando maior tensão para a porção clavicular (superior) do peitoral e deltoide anterior."),
        new(1, "Supino Inclinado com Halteres", "Empurrão inclinado com halteres em arco convergente natural, ideal para hipertrofia do peitoral superior com menor estresse articular."),
        new(1, "Supino Declinado com Barra", "Variação em banco declinado que enfatiza o recrutamento da porção esternocostal inferior do peitoral maior."),
        new(1, "Crucifixo Reto com Halteres", "Adução horizontal de ombros no banco reto com cotovelos semiflexionados, isolando as fibras médias do peitoral."),
        new(1, "Crucifixo Inclinado com Halteres", "Adução horizontal em banco inclinado (30°–45°) com foco no alongamento sob carga das fibras superiores do peitoral."),
        new(1, "Pullover com Halter", "Movimento no banco reto levando o halter atrás da cabeça com braços semirrígidos, trabalhando peitoral, serrátil anterior e caixa torácica."),
        new(1, "Flexão de Braços no Solo (Apoio)", "Exercício calistênico de cadeia cinética fechada para peitoral, tríceps e estabilização profunda do core."),

        // ==================== 2. COSTAS (8 Exercícios Livres / Peso Corporal) ====================
        new(2, "Barra Fixa Pronada (Pull-up)", "Puxada vertical com peso corporal em pegada aberta pronada, referência para largura dorsal e controle escapular."),
        new(2, "Barra Fixa Supinada (Chin-up)", "Puxada vertical com peso corporal em pegada fechada supinada, recrutando intensamente grande dorsal e bíceps."),
        new(2, "Remada Curvada Pronada com Barra", "Puxada horizontal livre com tronco inclinado, desenvolvendo espessura de grande dorsal, romboides e trapézio médio."),
        new(2, "Remada Curvada Supinada com Barra", "Variação estilo Yates Row com pegada supinada e tronco firme, enfatizando a porção inferior dos dorsais."),
        new(2, "Remada Unilateral com Halter (Serrote)", "Remada apoiada no banco para trabalho isolado de cada lado das costas, grande amplitude escapular e alívio lombar."),
        new(2, "Remada Cavalinho Livre com Barra T", "Remada pesada em barra T com pegada fechada ou pronada, excelente para densidade da região central das costas."),
        new(2, "Remada no Banco Inclinado com Halteres", "Remada bilateral com peito apoiado no banco a 30°–45°, isolando dorsais e romboides sem sobrecarga lombar."),
        new(2, "Pullover para Dorsais com Halter", "Extensão de ombros deitado com foco na depressão escapular e alongamento máximo das fibras do grande dorsal."),

        // ==================== 3. QUADRÍCEPS (8 Exercícios Livres / Peso Corporal) ====================
        new(3, "Agachamento Livre com Barra", "Exercício multiarticular base com barra apoiada no trapézio para força e hipertrofia global de quadríceps e glúteos."),
        new(3, "Agachamento Frontal com Barra", "Front Squat com barra apoiada nos deltoides anteriores e tronco vertical, maximizando a demanda sobre o quadríceps."),
        new(3, "Agachamento Goblet com Halter", "Agachamento segurando halter ou kettlebell junto ao peito, excelente para postura, mobilidade profunda e controle motor."),
        new(3, "Afundo Estacionário com Halteres", "Agachamento unilateral em base dividida com foco em estabilidade de joelho e quadril e força de quadríceps."),
        new(3, "Passada (Avanço) com Halteres", "Deslocamento dinâmico em passadas alternadas, gerando alto estímulo metabólico e recrutamento de coxas e glúteos."),
        new(3, "Agachamento Búlgaro (Ênfase Quadríceps)", "Agachamento unilateral com pé traseiro elevado no banco e tronco ereto para máxima flexão de joelho com halteres."),
        new(3, "Subida no Banco (Step-up) com Halteres", "Extensão unilateral de joelho e quadril subindo em banco ou caixa para força funcional e correção de assimetrias."),
        new(3, "Agachamento Sissy Livre", "Flexão de joelhos projetando as tíbias à frente com quadril estendido, isolando intensamente o reto femoral."),

        // ==================== 4. POSTERIOR DE COXA (6 Exercícios Livres / Peso Corporal) ====================
        new(4, "Stiff com Barra", "Extensão de quadril com joelhos semirrígidos e barra rente às pernas, alongando sob tensão os isquiotibiais e glúteos."),
        new(4, "Stiff com Halteres", "Variação com halteres à frente/lateral das coxas, permitindo ajuste ergonômico da pegada e excelente amplitude posterior."),
        new(4, "Levantamento Terra Romeno (RDL) com Barra", "Dobradiça de quadril com leve flexão de joelhos para alta sobrecarga excêntrica em posteriores de coxa e glúteos."),
        new(4, "Stiff Unilateral com Halter", "Execução apoiada em uma perna para correção de desequilíbrios musculares, equilíbrio pélvico e ativação posterior."),
        new(4, "Bom Dia (Good Morning) com Barra", "Flexão de quadril com barra apoiada nas costas, fortalecendo intensamente isquiotibiais, glúteos e paravertebrais."),
        new(4, "Flexão Nórdica (Nordic Curl)", "Exercício excêntrico de peso corporal com tornozelos fixos para hipertrofia e prevenção de lesões nos isquiotibiais."),

        // ==================== 5. GLÚTEOS (7 Exercícios Livres / Peso Corporal) ====================
        new(5, "Elevação Pélvica com Barra (Hip Thrust)", "Extensão de quadril com escápulas apoiadas no banco; principal exercício livre para força e pico de contração do glúteo máximo."),
        new(5, "Elevação Pélvica com Halter", "Versão prática no banco com halter apoiado sobre a pelve, ideal para iniciantes, intermediários e séries metabólicas."),
        new(5, "Agachamento Búlgaro (Ênfase Glúteos)", "Execução unilateral com passo longo e leve inclinação do tronco à frente para máximo alongamento sob carga do glúteo."),
        new(5, "Afundo Reverso (Recuo) com Halteres", "Passo controlado para trás que reduz o cisalhamento no joelho e acentua a ação extensora do glúteo máximo."),
        new(5, "Ponte de Glúteos no Solo com Barra", "Glute Bridge partindo do solo com menor participação de quadríceps e forte isolamento dos glúteos."),
        new(5, "Subida no Banco Alta (Step-up Glúteos)", "Subida em caixa ou banco alto com flexão profunda de quadril, recrutando intensamente as fibras inferiores do glúteo."),
        new(5, "Extensão de Quadril 4 Apoios com Caneleira", "Isolamento unilateral no solo com caneleira de peso para trabalho localizado e metabólico da musculatura glútea."),

        // ==================== 6. OMBROS (7 Exercícios Livres / Peso Corporal) ====================
        new(6, "Desenvolvimento com Halteres Sentado", "Empurrão vertical sentado com apoio dorsal, base para volume e força das porções anterior e medial do deltoide."),
        new(6, "Desenvolvimento Militar em Pé com Barra", "Empurrão vertical estrito com barra pela frente, desenvolvendo força da cintura escapular e estabilidade do core."),
        new(6, "Desenvolvimento Arnold com Halteres", "Desenvolvimento com rotação dos punhos da pegada supinada para pronada, recrutando amplamente o deltoide."),
        new(6, "Elevação Lateral com Halteres", "Abdução de ombros no plano escapular, principal movimento livre para largura e destaque do deltoide medial."),
        new(6, "Elevação Frontal Alternada com Halteres", "Flexão controlada de ombros à frente do tronco para estímulo direcionado à porção anterior do deltoide."),
        new(6, "Crucifixo Inverso Inclinado com Halteres", "Abdução horizontal com tronco inclinado ou apoiado no banco para isolamento do deltoide posterior e romboides."),
        new(6, "Elevação Lateral Inclinada no Banco", "Abdução unilateral apoiada no banco inclinado que altera o pico de torque e enfatiza o alongamento do deltoide lateral."),

        // ==================== 7. TRAPÉZIO (5 Exercícios Livres / Peso Corporal) ====================
        new(7, "Encolhimento de Ombros com Halteres", "Elevação escapular em pé segurando halteres ao lado do corpo para hipertrofia da porção superior do trapézio."),
        new(7, "Encolhimento de Ombros com Barra", "Elevação vertical das escápulas com barra longa pela frente para sobrecarga pesada no trapézio superior."),
        new(7, "Remada Alta com Barra W", "Puxada vertical controlada até a linha do esterno com barra W, trabalhando trapézio superior e deltoides."),
        new(7, "Encolhimento no Banco Inclinado com Halteres", "Variação apoiada de bruços no banco a 45° para atingir as fibras médias e superiores do trapézio com retração."),
        new(7, "Elevação em Y no Banco Inclinado", "Movimento escapular em Y deitado de bruços com halteres leves para fortalecimento do trapézio inferior e postura."),

        // ==================== 8. ADUTORES / ABDUTORES (6 Exercícios Livres / Peso Corporal) ====================
        new(8, "Agachamento Sumô com Halter", "Agachamento em base ampla com pés abduzidos segurando halter ao centro, recrutando adutores da coxa e glúteos."),
        new(8, "Levantamento Terra Sumô com Barra", "Puxada de solo em base aberta focada na força integrada de adutores magnos, glúteos e cadeia posterior."),
        new(8, "Afundo Lateral (Cossaco) com Halter", "Agachamento lateral unilateral para força e mobilidade ativa dos adutores e estabilizadores laterais do quadril."),
        new(8, "Prancha de Copenhague (Adutores)", "Isometria ou adução dinâmica lateral com perna superior apoiada no banco, excelente para fortalecimento dos adutores."),
        new(8, "Abdução de Quadril Deitado com Caneleira", "Elevação lateral da perna em decúbito lateral focada no glúteo médio, glúteo mínimo e estabilidade pélvica."),
        new(8, "Ostra (Clamshell) com Mini Band", "Rotação externa e abdução de quadril no solo com elástico para ativação profunda dos abdutores do quadril."),

        // ==================== 9. BÍCEPS (7 Exercícios Livres / Peso Corporal) ====================
        new(9, "Rosca Direta com Barra Reta", "Flexão de cotovelos bilateral em pé com supinação total para carga máxima nas duas cabeças do bíceps braquial."),
        new(9, "Rosca Direta com Barra W", "Variação anatômica em barra W que reduz o estresse nos punhos e antebraços mantendo alto estímulo nos bíceps."),
        new(9, "Rosca Alternada com Halteres", "Flexão unilateral alternada com supinação do punho, trabalhando pico de contração do bíceps e simetria muscular."),
        new(9, "Rosca Martelo com Halteres", "Flexão de cotovelos em pegada neutra focada no músculo braquial, braquiorradial e espessura lateral do braço."),
        new(9, "Rosca Inclinada 45° com Halteres", "Executada sentado no banco inclinado com ombros estendidos, maximizando o alongamento da cabeça longa do bíceps."),
        new(9, "Rosca Scott Livre com Barra W", "Braços apoiados à frente no banco Scott para isolar a cabeça curta do bíceps eliminando balanço do tronco."),
        new(9, "Rosca Concentrada com Halter", "Isolamento unilateral sentado apoiando o cotovelo na face interna da coxa para controle estrito da contração."),

        // ==================== 10. TRÍCEPS (7 Exercícios Livres / Peso Corporal) ====================
        new(10, "Tríceps Testa com Barra W", "Extensão de cotovelos deitado no banco reto ou inclinado, trabalhando intensamente as cabeças longa, lateral e medial."),
        new(10, "Tríceps Testa Neutro com Halteres", "Versão com halteres em pegada neutra que preserva cotovelos e punhos mantendo excelente amplitude excêntrica."),
        new(10, "Tríceps Francês Bilateral com Halter", "Extensão acima da cabeça com ombros flexionados para máximo alongamento e recrutamento da cabeça longa do tríceps."),
        new(10, "Tríceps Francês Unilateral com Halter", "Execução unilateral acima da cabeça para correção de assimetrias e trabalho profundo da cabeça longa do tríceps."),
        new(10, "Supino Fechado com Barra", "Movimento multiarticular no banco reto com pegada na largura dos ombros para alta carga e força bruta de tríceps."),
        new(10, "Mergulho nas Barras Paralelas Livre", "Exercício calistênico de peso corporal com tronco vertical para hipertrofia global de tríceps e peitoral inferior."),
        new(10, "Tríceps Coice com Halter", "Extensão de cotovelo com tronco inclinado paralelo ao solo, focando no encurtamento máximo final do tríceps."),

        // ==================== 11. PANTURRILHA (5 Exercícios Livres / Peso Corporal) ====================
        new(11, "Panturrilha em Pé Unilateral com Halter", "Flexão plantar unilateral em degrau com amplo alongamento excêntrico para simetria e foco no gastrocnêmio."),
        new(11, "Panturrilha em Pé Bilateral com Halteres", "Elevação plantar sobre step ou degrau segurando halteres para resistência e força das panturrilhas."),
        new(11, "Panturrilha em Pé com Barra Livre", "Flexão plantar pesada com barra apoiada sobre o trapézio para sobrecarga direta nos gastrocnêmios."),
        new(11, "Panturrilha Sentado Livre com Halteres", "Execução sentada apoiando halteres sobre os joelhos em 90° para enfatizar especificamente o músculo sóleo."),
        new(11, "Tibial Anterior Livre na Parede", "Dorsiflexão dos tornozelos com costas apoiadas na parede para fortalecer a canela e estabilizar o tornozelo."),

        // ==================== 12. ANTEBRAÇO (5 Exercícios Livres / Peso Corporal) ====================
        new(12, "Rosca Punho com Barra (Flexão de Punho)", "Flexão palmar sentado com antebraços apoiados nas coxas ou banco para hipertrofia dos flexores do antebraço."),
        new(12, "Rosca Punho Invertida com Barra", "Extensão dorsal dos punhos apoiado no banco para fortalecimento dos extensores do carpo e estabilidade articular."),
        new(12, "Rosca Inversa em Pé com Barra W", "Flexão de cotovelos em pegada pronada trabalhando intensamente o músculo braquiorradial e extensores."),
        new(12, "Caminhada do Fazendeiro (Farmer's Walk)", "Deslocamento segurando halteres pesados ao lado do corpo para força isométrica máxima de pegada, antebraço e core."),
        new(12, "Rolo de Punho (Wrist Roller)", "Enrolamento e desenrolamento de carga suspensa em bastão para resistência intensa de flexores e extensores."),

        // ==================== 13. ABDÔMEN (7 Exercícios Livres / Peso Corporal) ====================
        new(13, "Abdominal Supra no Solo (Crunch)", "Flexão torácica controlada no colchonete tirando as escápulas do chão com foco direto no reto abdominal."),
        new(13, "Abdominal Infra no Solo (Elevação de Pernas)", "Retroversão pélvica e elevação das pernas deitado, enfatizando a porção inferior do reto abdominal."),
        new(13, "Prancha Frontal Isométrica", "Sustentação corporal alinhada em antebraços e pés para ativação profunda do transverso do abdômen e core."),
        new(13, "Prancha Lateral Isométrica", "Estabilização lateral do tronco focada em oblíquos internos e externos, quadrado lombar e controle pélvico."),
        new(13, "Abdominal Bicicleta (Oblíquos)", "Flexão alternada com rotação torácica no solo recrutando simultaneamente reto abdominal e oblíquos."),
        new(13, "Rotação Russa (Russian Twist) com Anilha", "Giro controlado de tronco sentado segurando anilha ou halter para resistência rotacional dos oblíquos."),
        new(13, "Roda Abdominal (Ab Wheel)", "Anti-extensão dinâmica da coluna com rodinha no solo para fortalecimento intenso de toda a parede abdominal."),

        // ==================== 14. LOMBAR (5 Exercícios Livres / Peso Corporal) ====================
        new(14, "Levantamento Terra Convencional com Barra", "Exercício multiarticular de força global para eretores da espinha, cadeia posterior, trapézio e core."),
        new(14, "Superman no Solo", "Extensão simultânea de tronco, braços e pernas em decúbito ventral para fortalecimento seguro dos paravertebrais."),
        new(14, "Perdigueiro (Bird-Dog) em 4 Apoios", "Extensão cruzada de braço e perna mantendo coluna neutra para controle motor profundo dos multífidos e lombar."),
        new(14, "Ponte Isométrica de Cadeia Posterior", "Sustentação pélvica no solo para coativação de glúteos e estabilizadores lombares sem sobrecarga discal."),
        new(14, "Bom Dia com Halter Junto ao Peito", "Dobradiça controlada de quadril segurando halter junto ao esterno para resistência e postura dos eretores lombares.")
    ];

    public static readonly IReadOnlyList<ItemCatalogoPadrao> MaquinasPadrao =
    [
        // ==================== 1. PEITO (8 Máquinas / Polias) ====================
        new(1, "Supino Reto Máquina Articulada", "Máquina convergente guiada por anilhas para empurrão horizontal seguro, alta estabilidade e tensão constante no peitoral."),
        new(1, "Supino Inclinado Máquina Articulada", "Equipamento articulado convergente focado na porção superior (clavicular) do peitoral com trajetória guiada."),
        new(1, "Supino Declinado Máquina Articulada", "Aparelho articulado com vetor de empurrão inferior para ênfase na borda esternocostal inferior do peitoral."),
        new(1, "Supino Sentado Máquina (Chest Press)", "Máquina seletorizada por bateria de pesos, ideal para controle motor seguro de iniciantes e séries até a falha."),
        new(1, "Voador Peitoral (Peck Deck)", "Aparelho isolador de adução horizontal para máxima contração de pico do peitoral sem fadiga limitante dos tríceps."),
        new(1, "Crossover Polia Alta", "Crucifixo em pé nos cabos partindo da polia alta em direção ao quadril, enfatizando fibras médias e inferiores do peitoral."),
        new(1, "Crossover Polia Baixa", "Adução ascendente nos cabos partindo da polia baixa, focando na porção clavicular (superior) do peitoral."),
        new(1, "Supino Inclinado no Smith (Barra Guiada)", "Empurrão inclinado nos trilhos da máquina Smith, unindo alta sobrecarga e travas de segurança para isolamento peitoral."),

        // ==================== 2. COSTAS (9 Máquinas / Polias) ====================
        new(2, "Puxada Alta Aberta (Pulley Frente)", "Puxada vertical na polia alta com barra longa pronada para desenvolvimento da largura do grande dorsal."),
        new(2, "Puxada Alta com Triângulo (Pegada Neutra)", "Puxada vertical fechada na polia alta, favorecendo o alongamento e recrutamento das fibras inferiores do dorsal."),
        new(2, "Puxada Alta Supinada no Pulley", "Variação na polia alta com palmas voltadas para o rosto, aumentando a amplitude de extensão dos ombros."),
        new(2, "Remada Baixa Sentada no Cabo (Triângulo)", "Puxada horizontal sentada na polia inferior com retração escapular completa para espessura das costas."),
        new(2, "Remada Baixa com Barra Romana", "Variação na polia baixa com pegada neutra média ou aberta, enfatizando romboides, redondo maior e meio das costas."),
        new(2, "Remada Máquina Articulada (Apoio no Peito)", "Equipamento convergente com suporte torácico que estabiliza a coluna e permite cargas elevadas nos dorsais."),
        new(2, "Puxada Alta Máquina Articulada", "Máquina plate-loaded de alavancas independentes superiores para trabalho simétrico e pesado do grande dorsal."),
        new(2, "Pulldown na Polia Alta com Corda ou Barra", "Extensão de ombros em pé com cotovelos semiestendidos no cabo, isolando o grande dorsal sem ação dos bíceps."),
        new(2, "Barra Fixa Assistida no Gráviton", "Máquina contrabalançada por bateria de pesos que auxilia a puxada vertical para progressão técnica segura."),

        // ==================== 3. QUADRÍCEPS (8 Máquinas / Polias) ====================
        new(3, "Leg Press 45°", "Prensa inclinada guiada por trilhos para alta sobrecarga mecânica em quadríceps e glúteos com coluna totalmente apoiada."),
        new(3, "Leg Press Horizontal (Sentado)", "Máquina seletorizada horizontal ergonômica e segura, excelente para iniciantes, idosos e execuções unilaterais."),
        new(3, "Cadeira Extensora", "Aparelho isolador de extensão de joelhos para recrutamento direto dos quatro ventres do músculo quadríceps."),
        new(3, "Agachamento Hack (Hack Machine)", "Máquina inclinada sobre trilhos com suporte dorsal completo, permitindo agachar profundo com foco estrito em quadríceps."),
        new(3, "Agachamento no Smith (Barra Guiada)", "Agachamento guiado em trilhos com travas de segurança, versátil para ajuste de base dos pés e estabilidade."),
        new(3, "Agachamento Pêndulo (Pendulum Squat)", "Máquina articulada em arco biomecânico que alivia a lombar e maximiza a amplitude de flexão do joelho."),
        new(3, "V-Squat Articulado", "Equipamento de alavanca com apoio acolchoado nos ombros para agachamento pesado em trajetória guiada."),
        new(3, "Belt Squat (Agachamento no Cinto)", "Máquina onde a carga traciona pelo quadril via cinturão, treinando pernas intensamente sem compressão na coluna."),

        // ==================== 4. POSTERIOR DE COXA (6 Máquinas / Polias) ====================
        new(4, "Mesa Flexora (Flexor Deitado)", "Aparelho em decúbito ventral para flexão de joelhos bilateral ou unilateral com foco direto nos isquiotibiais."),
        new(4, "Cadeira Flexora (Flexor Sentado)", "Máquina sentada que mantém o quadril flexionado, pré-alongando os isquiotibiais para ganho superior de hipertrofia."),
        new(4, "Flexora em Pé Unilateral (Vertical)", "Equipamento isolador unilateral em pé para simetria e contração máxima da musculatura posterior da coxa."),
        new(4, "Stiff no Smith (Barra Guiada)", "Dobradiça de quadril guiada nos trilhos do Smith, garantindo trajetória estável e foco excêntrico nos posteriores."),
        new(4, "Stiff na Polia Baixa (Cabo)", "Extensão de quadril puxando barra ou corda na polia inferior com tensão constante em toda a amplitude."),
        new(4, "Glute Ham Raise no Banco GHD", "Extensão combinada de quadril e flexão de joelhos no aparelho GHD para força integrada da cadeia posterior."),

        // ==================== 5. GLÚTEOS (7 Máquinas / Polias) ====================
        new(5, "Elevação Pélvica Máquina (Hip Thrust Machine)", "Equipamento dedicado com cinto ou rolo acolchoado que facilita o posicionamento e a progressão segura de carga no glúteo."),
        new(5, "Glúteo Máquina 4 Apoios (Coice Guiado)", "Aparelho específico de extensão de quadril para trás, isolando o glúteo máximo com tronco e lombar estabilizados."),
        new(5, "Glúteo em Pé Máquina (Chute Traseiro)", "Máquina seletorizada vertical para extensão unilateral do quadril com suporte peitoral ergonômico."),
        new(5, "Coice de Glúteo na Polia Baixa (Apolete)", "Extensão de quadril com tornozeleira presa ao cabo inferior, mantendo tensão contínua do alongamento ao pico."),
        new(5, "Extensão Diagonal de Glúteo na Polia Baixa", "Chute póstero-lateral a 30°–45° no cabo com tornozeleira para atingir glúteo máximo e fibras superiores."),
        new(5, "Glúteo Unilateral no Gráviton", "Empurrão descendente unilateral sobre a plataforma do Gráviton para intensa ativação de glúteo máximo e posterior."),
        new(5, "Afundo Reverso no Smith (Barra Guiada)", "Recuo unilateral guiado nos trilhos do Smith, permitindo focar totalmente na extensão do quadril com equilíbrio."),

        // ==================== 6. OMBROS (7 Máquinas / Polias) ====================
        new(6, "Desenvolvimento de Ombros Máquina", "Shoulder Press seletorizado sentado com pegadas pronada e neutra para sobrecarga segura nos deltoides."),
        new(6, "Desenvolvimento de Ombros Articulado", "Máquina convergente por anilhas com braços independentes para força simétrica da cintura escapular."),
        new(6, "Elevação Lateral Máquina", "Aparelho seletorizado com apoio nos cotovelos que isola o deltoide medial eliminando compensações nos punhos."),
        new(6, "Elevação Lateral Unilateral na Polia Baixa", "Abdução de ombro no cabo mantendo tensão constante desde a fase inicial de alongamento junto ao quadril."),
        new(6, "Crucifixo Inverso Máquina (Peck Deck Invertido)", "Execução voltada para o encosto do voador para isolamento estrito do deltoide posterior e manguito externo."),
        new(6, "Face Pull na Polia Alta com Corda", "Puxada em direção à face com rotação externa de ombros para deltoide posterior, estabilidade escapular e postura."),
        new(6, "Elevação Frontal na Polia Baixa com Corda", "Flexão de ombros no cabo inferior com pegada neutra, garantindo tensão contínua no deltoide anterior."),

        // ==================== 7. TRAPÉZIO (5 Máquinas / Polias) ====================
        new(7, "Encolhimento de Ombros no Smith", "Elevação escapular nos trilhos da barra guiada pela frente ou por trás, oferecendo estabilidade para cargas altas."),
        new(7, "Encolhimento Máquina Articulada", "Shrug Machine carregada por anilhas com pegadas laterais neutras ergonômicas para isolamento pesado do trapézio."),
        new(7, "Encolhimento na Polia Baixa com Barra", "Elevação escapular no cabo inferior com vetor contínuo levemente diagonal favorável às fibras do trapézio."),
        new(7, "Remada Alta na Polia Baixa", "Puxada vertical guiada pelo cabo com barra reta ou corda, mantendo tensão constante em trapézio e deltoides."),
        new(7, "Depressão Escapular na Polia Alta", "Retração e depressão controlada das escápulas no pulley com braços estendidos para foco no trapézio inferior."),

        // ==================== 8. ADUTORES / ABDUTORES (6 Máquinas / Polias) ====================
        new(8, "Cadeira Adutora", "Máquina seletorizada sentada de fechamento das pernas para isolamento direto da musculatura interna da coxa."),
        new(8, "Cadeira Abdutora", "Máquina seletorizada sentada de abertura das pernas para fortalecimento de glúteo médio, mínimo e lateral do quadril."),
        new(8, "Cadeira Abdutora Tronco Inclinado", "Variação postural na cadeira abdutora que enfatiza as fibras póstero-superiores do glúteo médio e máximo."),
        new(8, "Abdutora em Pé Articulada (3D)", "Equipamento articulado de abdução em semi-agachamento para alta sobrecarga na musculatura lateral dos glúteos."),
        new(8, "Abdução de Quadril na Polia Baixa", "Movimento lateral unilateral em pé com tornozeleira no cabo inferior para tensão contínua nos abdutores."),
        new(8, "Adução de Quadril na Polia Baixa", "Cruzamento medial da perna à frente do corpo com tornozeleira no cabo para trabalho unilateral dos adutores."),

        // ==================== 9. BÍCEPS (6 Máquinas / Polias) ====================
        new(9, "Rosca Scott Máquina", "Aparelho guiado com apoio inclinado de braços que mantém resistência uniforme em toda a flexão dos cotovelos."),
        new(9, "Rosca Direta na Polia Baixa", "Flexão de cotovelos com barra reta ou W no cabo inferior garantindo tensão contínua mesmo no topo do movimento."),
        new(9, "Rosca Martelo na Polia Baixa com Corda", "Puxada em pegada neutra com corda no cabo para estímulo constante de bíceps, braquial e antebraço."),
        new(9, "Rosca Unilateral na Polia Baixa (Bayesiana)", "Executada à frente da polia baixa com ombro estendido atrás do tronco para máximo alongamento sob tensão do bíceps."),
        new(9, "Duplo Bíceps na Polia Alta (Crossover)", "Flexão bilateral partindo das polias altas para ênfase em encurtamento e pico de contração dos bíceps."),
        new(9, "Bíceps Máquina Articulada Unilateral", "Equipamento plate-loaded independente para sobrecarga progressiva controlada sem compensação da coluna."),

        // ==================== 10. TRÍCEPS (7 Máquinas / Polias) ====================
        new(10, "Tríceps Pulley na Polia Alta (Barra Reta ou V)", "Extensão de cotovelos clássica no cabo alto com ênfase na cabeça lateral do tríceps e alta estabilidade."),
        new(10, "Tríceps Corda na Polia Alta", "Extensão no pulley abrindo a corda no final da fase concêntrica para pico de contração das três cabeças do tríceps."),
        new(10, "Tríceps Francês na Polia com Corda", "Extensão acima da cabeça de costas para o cabo, unindo alongamento da cabeça longa e tensão constante."),
        new(10, "Tríceps Máquina Sentado (Paralela Máquina)", "Dip Machine guiada de empurrão descendente sentado, segura e eficiente para cargas elevadas no tríceps."),
        new(10, "Tríceps Unilateral Supinado na Polia", "Extensão unilateral em pegada invertida no cabo para isolamento fino e controle articular de cotovelo."),
        new(10, "Tríceps Coice na Polia Baixa", "Versão no cabo do tríceps coice que mantém resistência desde o início da extensão até o bloqueio final."),
        new(10, "Mergulho Assistido no Gráviton", "Paralelas com contrapeso na máquina Gráviton para execução técnica controlada em iniciantes e intermediários."),

        // ==================== 11. PANTURRILHA (5 Máquinas / Polias) ====================
        new(11, "Panturrilha em Pé Máquina (Gêmeos em Pé)", "Aparelho vertical com apoio nos ombros e joelhos estendidos para máxima hipertrofia do músculo gastrocnêmio."),
        new(11, "Panturrilha Sentada Máquina (Banco Solear)", "Equipamento articulado sentado com joelhos flexionados a 90°, isolando predominantemente o músculo sóleo."),
        new(11, "Panturrilha no Leg Press 45°", "Flexão plantar na borda inferior da plataforma do Leg Press 45° com alto potencial de sobrecarga segura."),
        new(11, "Panturrilha no Leg Press Horizontal", "Variação guiada em máquina seletorizada horizontal com excelente controle da fase excêntrica alongada."),
        new(11, "Panturrilha no Smith sobre Step", "Elevação plantar nos trilhos da barra guiada com ponta dos pés apoiada em step para equilíbrio total sob carga."),

        // ==================== 12. ANTEBRAÇO (4 Máquinas / Polias) ====================
        new(12, "Rosca Inversa na Polia Baixa", "Flexão de cotovelos pronada com barra reta ou W no cabo inferior, mantendo tensão constante no braquiorradial."),
        new(12, "Rosca Punho na Polia Baixa", "Flexão palmar de punhos com barra reta conectada ao cabo baixo e antebraços estabilizados."),
        new(12, "Extensão de Punho na Polia Baixa", "Trabalho isolado dos extensores do antebraço no cabo para equilíbrio muscular e proteção do cotovelo."),
        new(12, "Suspensão Isométrica na Barra do Crossover", "Dead Hang pendurado na estrutura para resistência isométrica de pegada, antebraços e descompressão escapular."),

        // ==================== 13. ABDÔMEN (6 Máquinas / Polias) ====================
        new(13, "Abdominal Máquina Sentado (Crunch Máquina)", "Aparelho seletorizado guiado para flexão de tronco com progressão precisa de carga para hipertrofia abdominal."),
        new(13, "Abdominal Supra na Polia Alta com Corda", "Flexão da coluna ajoelhado puxando a corda no cabo alto para tensão contínua no reto abdominal."),
        new(13, "Abdominal Infra na Cadeira Paralela", "Elevação de joelhos ou pernas na torre vertical (Captain's Chair) com coluna e antebraços estabilizados."),
        new(13, "Abdominal no Banco Declinado", "Flexão de tronco em prancha declinada regulável com roletes para os pés, permitindo progressão com anilha."),
        new(13, "Pallof Press na Polia Média", "Estabilização anti-rotação contra tração lateral do cabo para proteção vertebral e força profunda do core."),
        new(13, "Máquina Rotatória de Tronco (Oblíquos)", "Equipamento seletorizado sentado para rotação controlada da coluna torácica focando nos músculos oblíquos."),

        // ==================== 14. LOMBAR (4 Máquinas / Polias) ====================
        new(14, "Hiperextensão Lombar no Banco Romano 45°", "Extensão controlada de quadril e tronco no banco 45° para resistência de paravertebrais lombares e glúteos."),
        new(14, "Extensão Lombar Máquina Sentado", "Back Extension seletorizada com apoio escapular para fortalecimento guiado e seguro dos eretores da espinha."),
        new(14, "Hiperextensão no Banco Romano 90°", "Extensão horizontal da cadeia posterior em banco 90° exigindo alta ativação de lombar, glúteos e posteriores."),
        new(14, "Meio Levantamento Terra no Smith (Rack Pull)", "Puxada parcial a partir da altura dos joelhos na barra guiada para força isométrica lombar e dorsal.")
    ];

    /// <summary>
    /// Chamado na inicialização da API: garante que TODAS as contas antigas já existentes no banco
    /// recebam os exercícios e máquinas pré-cadastrados sem duplicar o que o Personal já cadastrou.
    /// </summary>
    public static async Task InicializarContasPendentesAsync(AppDbContext db)
    {
        var contasPendentes = await db.Usuarios
            .Where(u => !u.CatalogoInicializado)
            .ToListAsync();

        if (contasPendentes.Count == 0)
        {
            return;
        }

        foreach (var conta in contasPendentes)
        {
            await PovoarItensFaltantesSemSalvarAsync(db, conta);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Garante que uma conta específica de Personal (recém-criada ou antiga no login)
    /// receba toda a carga pré-cadastrada de Exercícios e Máquinas.
    /// Se a conta já foi inicializada antes, respeita 100% as exclusões e edições feitas pelo Personal.
    /// </summary>
    public static async Task GarantirCatalogoDoPersonalAsync(AppDbContext db, Usuario conta)
    {
        if (conta.CatalogoInicializado)
        {
            return;
        }

        await PovoarItensFaltantesSemSalvarAsync(db, conta);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Guard rápido por ID do usuário (ex.: sessões já logadas no navegador que chamam GET /exercicios ou GET /maquinas).
    /// </summary>
    public static async Task GarantirCatalogoPorUsuarioIdAsync(AppDbContext db, int usuarioId)
    {
        if (usuarioId <= 0)
        {
            return;
        }

        var contaPendente = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId && !u.CatalogoInicializado);

        if (contaPendente is not null)
        {
            await GarantirCatalogoDoPersonalAsync(db, contaPendente);
        }
    }

    /// <summary>
    /// Permite que o Personal restaure os exercícios pré-cadastrados que tenha removido,
    /// sem apagar nem alterar os exercícios personalizados criados por ele.
    /// </summary>
    public static async Task<(int inseridos, int reativados)> RestaurarExerciciosPadraoAsync(AppDbContext db, int usuarioId)
    {
        var existentes = await db.Exercicios
            .Where(e => e.UsuarioId == usuarioId)
            .ToListAsync();

        var mapaPorNome = existentes
            .GroupBy(e => NormalizarNome(e.Nome))
            .ToDictionary(g => g.Key, g => g.First());

        int inseridos = 0;
        int reativados = 0;

        foreach (var item in ExerciciosPadrao)
        {
            var chave = NormalizarNome(item.Nome);
            if (mapaPorNome.TryGetValue(chave, out var existente))
            {
                if (!existente.Ativo)
                {
                    existente.Ativo = true;
                    reativados++;
                }
            }
            else
            {
                db.Exercicios.Add(new Exercicio
                {
                    Nome = item.Nome,
                    GrupoMuscularId = item.GrupoMuscularId,
                    Descricao = item.Descricao,
                    UsuarioId = usuarioId,
                    Ativo = true
                });
                inseridos++;
            }
        }

        var conta = await db.Usuarios.FindAsync(usuarioId);
        if (conta is not null && !conta.CatalogoInicializado)
        {
            conta.CatalogoInicializado = true;
        }

        if (inseridos > 0 || reativados > 0 || (conta is not null))
        {
            await db.SaveChangesAsync();
        }

        return (inseridos, reativados);
    }

    /// <summary>
    /// Permite que o Personal restaure as máquinas pré-cadastradas que tenha removido,
    /// sem apagar nem alterar as máquinas personalizadas criadas por ele.
    /// </summary>
    public static async Task<(int inseridos, int reativados)> RestaurarMaquinasPadraoAsync(AppDbContext db, int usuarioId)
    {
        var existentes = await db.Maquinas
            .Where(m => m.UsuarioId == usuarioId)
            .ToListAsync();

        var mapaPorNome = existentes
            .GroupBy(m => NormalizarNome(m.Nome))
            .ToDictionary(g => g.Key, g => g.First());

        int inseridos = 0;
        int reativados = 0;

        foreach (var item in MaquinasPadrao)
        {
            var chave = NormalizarNome(item.Nome);
            if (mapaPorNome.TryGetValue(chave, out var existente))
            {
                if (!existente.Ativo)
                {
                    existente.Ativo = true;
                    reativados++;
                }
            }
            else
            {
                db.Maquinas.Add(new Maquina
                {
                    Nome = item.Nome,
                    GrupoMuscularId = item.GrupoMuscularId,
                    Descricao = item.Descricao,
                    UsuarioId = usuarioId,
                    Ativo = true
                });
                inseridos++;
            }
        }

        var conta = await db.Usuarios.FindAsync(usuarioId);
        if (conta is not null && !conta.CatalogoInicializado)
        {
            conta.CatalogoInicializado = true;
        }

        if (inseridos > 0 || reativados > 0 || (conta is not null))
        {
            await db.SaveChangesAsync();
        }

        return (inseridos, reativados);
    }

    private static async Task PovoarItensFaltantesSemSalvarAsync(AppDbContext db, Usuario conta)
    {
        var nomesExerciciosExistentes = (await db.Exercicios
            .Where(e => e.UsuarioId == conta.Id)
            .Select(e => e.Nome)
            .ToListAsync())
            .Select(NormalizarNome)
            .ToHashSet();

        foreach (var item in ExerciciosPadrao)
        {
            if (!nomesExerciciosExistentes.Contains(NormalizarNome(item.Nome)))
            {
                db.Exercicios.Add(new Exercicio
                {
                    Nome = item.Nome,
                    GrupoMuscularId = item.GrupoMuscularId,
                    Descricao = item.Descricao,
                    UsuarioId = conta.Id,
                    Ativo = true
                });
            }
        }

        var nomesMaquinasExistentes = (await db.Maquinas
            .Where(m => m.UsuarioId == conta.Id)
            .Select(m => m.Nome)
            .ToListAsync())
            .Select(NormalizarNome)
            .ToHashSet();

        foreach (var item in MaquinasPadrao)
        {
            if (!nomesMaquinasExistentes.Contains(NormalizarNome(item.Nome)))
            {
                db.Maquinas.Add(new Maquina
                {
                    Nome = item.Nome,
                    GrupoMuscularId = item.GrupoMuscularId,
                    Descricao = item.Descricao,
                    UsuarioId = conta.Id,
                    Ativo = true
                });
            }
        }

        conta.CatalogoInicializado = true;
    }

    public static string NormalizarNome(string? nome)
    {
        return (nome ?? string.Empty).Trim().ToLowerInvariant();
    }
}
