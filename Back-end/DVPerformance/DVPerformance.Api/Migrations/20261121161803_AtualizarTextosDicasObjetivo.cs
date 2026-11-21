using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarTextosDicasObjetivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 1,
                column: "texto",
                value: "Segure a descida do peso com controle em cada repetição. Não deixe o peso despencar: controlar a volta força o músculo a trabalhar o dobro e gera muito mais resultado.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 2,
                column: "texto",
                value: "Tente aumentar um pouco o peso ou fazer 1 ou 2 repetições a mais com o passar das semanas, mas só suba a carga quando conseguir fazer o movimento completo e bem feito.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 3,
                column: "texto",
                value: "Respeite o tempo de descanso marcado na ficha. Descansar o tempo certo recupera sua energia para você conseguir pegar firme na próxima série.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 4,
                column: "texto",
                value: "O músculo cresce quando você descansa, não durante o treino. Procure dormir de 7 a 9 horas por noite para seu corpo se recuperar e construir massa muscular.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 5,
                column: "texto",
                value: "Inclua boas fontes de proteína nas suas refeições ao longo do dia e beba bastante água: é disso que seu corpo precisa para construir músculos.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 6,
                column: "texto",
                value: "Treine até sentir que o músculo realmente cansou nas últimas repetições, mas sem roubar no movimento ou dar impulsos com o corpo para levantar o peso.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 7,
                column: "texto",
                value: "Foque no músculo que está treinando em vez de apenas mover o peso no automático. Sinta ele alongar e contrair bem a cada repetição.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 8,
                column: "texto",
                value: "Faça o cardio depois da musculação no ritmo moderado indicado na ficha. Isso melhora seu fôlego e ajuda o corpo a absorver melhor os nutrientes sem atrapalhar o ganho de massa.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 9,
                column: "texto",
                value: "Nunca pule o aquecimento antes de pegar pesado. Fazer 1 ou 2 séries leves prepara suas articulações, evita lesões e melhora sua força no treino.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 10,
                column: "texto",
                value: "Mais vale treinar bem toda semana do que fazer um treino exagerado e faltar nos outros dias. A regularidade é o que traz resultados visíveis.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 11,
                column: "texto",
                value: "Anote os pesos que você usa nos exercícios principais. Assim você acompanha sua evolução de verdade e sabe exatamente quando está na hora de aumentar a carga.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 12,
                column: "texto",
                value: "Não treine o mesmo músculo em dias seguidos. Siga a divisão da sua ficha para dar o descanso necessário para o músculo crescer mais forte.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 13,
                column: "texto",
                value: "Fique de olho no relógio e respeite os descansos curtos da ficha. Isso mantém seus batimentos acelerados e faz você gastar muito mais calorias no treino.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 14,
                column: "texto",
                value: "Musculação também emagrece! Manter e ganhar músculos deixa seu metabolismo mais acelerado até em repouso e ajuda a evitar a flacidez enquanto você perde peso.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 15,
                column: "texto",
                value: "Fazer o cardio logo após a musculação turbina a queima de gordura, já que seu corpo já gastou a energia rápida durante os exercícios com peso.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 16,
                column: "texto",
                value: "Mexa-se mais fora da academia: troque o elevador pela escada, caminhe sempre que puder e evite ficar sentado por horas. Esses pequenos hábitos fazem muita diferença na balança.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 17,
                column: "texto",
                value: "Beba bastante água ao longo do dia e durante o treino. Além de melhorar sua disposição, a água ajuda o metabolismo a funcionar bem e controla a sensação de fome.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 18,
                column: "texto",
                value: "Não pegue leve demais nos pesos só porque quer emagrecer. Treinar com intensidade mostra ao seu corpo que ele precisa queimar gordura, e não os seus músculos.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 19,
                column: "texto",
                value: "O treino anda junto com a alimentação: prefira comidas naturais, frutas, verduras e boas proteínas no dia a dia, sem precisar passar fome com dietas radicais.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 20,
                column: "texto",
                value: "Dormir bem ajuda a emagrecer: noites mal dormidas aumentam o estresse e a vontade de comer doces e besteiras no dia seguinte, além de tirarem sua energia para treinar.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 21,
                column: "texto",
                value: "Cuidado para não exagerar na comida achando que 'já queimou tudo na academia'. É o equilíbrio diário entre treino e alimentação que transforma seu corpo.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 22,
                column: "texto",
                value: "Não se prenda só ao número da balança! Observe como suas roupas estão vestindo, suas medidas e sua disposição, pois você pode estar trocando gordura por músculo.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 23,
                column: "texto",
                value: "Capriche nos exercícios que trabalham vários músculos ao mesmo tempo (como agachamento, leg press e puxadas). Eles exigem mais do corpo e queimam muito mais calorias.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 24,
                column: "texto",
                value: "Nos dias de preguiça, vá treinar mesmo assim, nem que seja em um ritmo mais leve. Manter o hábito ativo vale muito mais do que esperar pelo dia perfeito.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 25,
                column: "texto",
                value: "No treino de força, o descanso maior entre as séries é obrigatório. Espere o tempo completo marcado na ficha para recuperar totalmente a energia antes de levantar pesado de novo.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 26,
                column: "texto",
                value: "Trave bem o abdômen e respire fundo antes de iniciar cada repetição pesada. Um tronco firme protege sua coluna e ajuda você a empurrar ou puxar mais peso com segurança.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 27,
                column: "texto",
                value: "Antes de colocar sua carga máxima no primeiro exercício, faça algumas séries de aquecimento aumentando o peso aos poucos. Isso prepara seu corpo e suas articulações para o esforço.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 28,
                column: "texto",
                value: "Desça o peso com total controle e faça força para subir com firmeza e explosão. Essa intenção de velocidade ativa mais fibras musculares e aumenta sua força.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 29,
                column: "texto",
                value: "Técnica sempre vem antes do peso: se perceber que a postura entortou ou que você está forçando a coluna para subir a carga, pare a série e ajuste o peso.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 30,
                column: "texto",
                value: "Antes de começar o movimento, firme bem os pés no chão, encaixe os ombros e segure a barra com firmeza. Uma base estável evita desperdício de energia.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 31,
                column: "texto",
                value: "Não tenha pressa de dobrar os pesos. Aumentar de 1 kg em 1 kg ou 2 kg em 2 kg toda semana gera um ganho enorme de força sem machucar suas articulações.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 32,
                column: "texto",
                value: "Dê o seu máximo nos primeiros exercícios da ficha (como supino, agachamento e remadas), pois você ainda está descansado e são eles que constroem sua base de força.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 33,
                column: "texto",
                value: "Mantenha o cardio em ritmo leve, apenas como indicado na ficha. O objetivo aqui é cuidar do coração sem gastar a energia que você precisa para levantar cargas altas.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 34,
                column: "texto",
                value: "Faça exercícios de mobilidade para ombros, quadris e tornozelos. Articulações soltas e flexíveis permitem movimentos mais profundos, seguros e com muito mais força.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 35,
                column: "texto",
                value: "Respeite os dias de descanso da semana. Levantar cargas altas exige muito do corpo, e é no repouso que você realmente fica mais forte para o próximo treino.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 36,
                column: "texto",
                value: "Não vá treinar força em jejum ou desidratado. Alimente-se bem e beba água antes do treino para ter energia suficiente na hora de encarar os pesos maiores.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 37,
                column: "texto",
                value: "Mantenha um ritmo constante e respire sem prender o ar durante as séries longas. Segurar a respiração faz você cansar muito mais rápido.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 38,
                column: "texto",
                value: "Escolha um peso que permita completar todas as repetições altas da ficha mantendo a postura correta do começo ao fim, sem precisar parar no meio da série.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 39,
                column: "texto",
                value: "Respeite os intervalos curtos de descanso entre as séries. É exatamente treinar com o músculo ainda um pouco cansado que aumenta sua resistência física.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 40,
                column: "texto",
                value: "Beba água antes, durante e depois do treino. Em treinos longos e com muitas repetições, a falta de água derruba seu fôlego e sua energia rapidamente.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 41,
                column: "texto",
                value: "Faça o cardio contínuo após o treino no ritmo indicado na ficha. Ele fortalece seu coração, aumenta o fôlego e ajuda seu corpo a se recuperar mais rápido do cansaço.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 42,
                column: "texto",
                value: "Quando o músculo começar a queimar nas repetições finais, não encurte o movimento. Manter a amplitude completa mesmo cansado é o que desenvolve resistência de verdade.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 43,
                column: "texto",
                value: "Inclua carboidratos saudáveis na sua alimentação (como arroz, aveia, frutas e raízes). Eles são o combustível principal para aguentar treinos longos sem perder o ritmo.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 44,
                column: "texto",
                value: "Relaxe o pescoço e não tensione os ombros à toa durante os exercícios. Evitar tensão desnecessária economiza energia para você ir mais longe na série.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 45,
                column: "texto",
                value: "Você sabe que está evoluindo quando o mesmo treino começa a parecer mais fácil e sua respiração volta ao normal cada vez mais rápido entre uma série e outra.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 46,
                column: "texto",
                value: "Tire 5 minutinhos para fazer alongamentos leves ao final do treino. Isso alivia a rigidez muscular e deixa seu corpo mais solto para as próximas sessões da semana.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 47,
                column: "texto",
                value: "A regularidade é o segredo do fôlego: quanto mais constante você for na semana, mais rápido seu corpo se adapta e demora mais para cansar.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 48,
                column: "texto",
                value: "Nas últimas repetições, quando o cansaço bater, redobre a atenção na postura e mantenha o abdômen firme para não sobrecarregar a lombar.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 1,
                column: "texto",
                value: "Priorize a execução controlada na fase excêntrica (descida do peso) em cada repetição, mantendo a tensão mecânica constante no músculo-alvo para maximizar o estímulo de hipertrofia.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 2,
                column: "texto",
                value: "Busque a progressão gradual de carga ou de repetições semana após semana, sempre preservando a amplitude completa do movimento antes de aumentar o peso.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 3,
                column: "texto",
                value: "Respeite os intervalos de descanso prescritos entre as séries para recuperar sua capacidade de contração e manter um alto volume de carga útil durante toda a sessão.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 4,
                column: "texto",
                value: "O crescimento muscular acontece durante a recuperação: priorize de 7 a 9 horas de sono reparador por noite para otimizar a síntese proteica e a regeneração das fibras.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 5,
                column: "texto",
                value: "Mantenha uma ingestão adequada de proteínas fracionada ao longo do dia e hidratação constante para fornecer os nutrientes necessários à construção de massa magra.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 6,
                column: "texto",
                value: "Leve as séries de trabalho próximas à falha concêntrica com segurança técnica, evitando compensações posturais ou impulsos que roubem o esforço do músculo principal.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 7,
                column: "texto",
                value: "Concentre-se na conexão mente-músculo durante cada série, sentindo o alongamento completo e o pico de contração antes de iniciar a próxima repetição.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 8,
                column: "texto",
                value: "Realize o cardio pós-treino na intensidade moderada indicada na ficha para melhorar a sensibilidade à insulina e o condicionamento sem comprometer a recuperação muscular.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 9,
                column: "texto",
                value: "Não pule as séries iniciais de aquecimento articular e específico: elas preparam tendões e sistema nervoso para suportar cargas mais produtivas nas séries principais.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 10,
                column: "texto",
                value: "A constância semanal supera treinos isolados extremos: cumprir todas as sessões planejadas garante a frequência ideal de estímulo para cada grupamento muscular.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 11,
                column: "texto",
                value: "Anote as cargas utilizadas nos exercícios base do seu protocolo para acompanhar sua evolução real e identificar o momento exato de progredir nos treinos seguintes.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 12,
                column: "texto",
                value: "Evite treinar o mesmo grupo muscular em dias consecutivos fora da divisão prescrita, garantindo a supercompensação necessária para o ganho sólido de volume muscular.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 13,
                column: "texto",
                value: "Mantenha os intervalos de descanso curtos e rigorosos conforme prescrito na ficha para sustentar a frequência cardíaca elevada e ampliar o gasto calórico da sessão.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 14,
                column: "texto",
                value: "A musculação é essencial no emagrecimento: preservar e estimular sua massa magra mantém o metabolismo basal ativo e evita a flacidez durante a redução de gordura.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 15,
                column: "texto",
                value: "Combine o treino de força com o cardio pós-treino indicado no protocolo para potencializar a oxidação de gorduras aproveitando a depleção prévia de glicogênio.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 16,
                column: "texto",
                value: "Aumente seu nível de movimento fora da academia: caminhar mais no dia a dia, usar escadas e evitar longos períodos sentado acelera significativamente o gasto energético semanal.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 17,
                column: "texto",
                value: "Mantenha excelente hidratação antes, durante e após o treino; o consumo adequado de água otimiza o metabolismo energético, o desempenho físico e o controle da saciedade.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 18,
                column: "texto",
                value: "Não reduza drasticamente as cargas por estar em foco de perda de gordura: manter treinos intensos sinaliza ao organismo que a musculatura deve ser preservada.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 19,
                column: "texto",
                value: "Alinhe a regularidade dos treinos a uma alimentação equilibrada em déficit calórico sustentável, priorizando alimentos naturais, fibras e bom aporte de proteínas.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 20,
                column: "texto",
                value: "Cuide da qualidade do sono diariamente: noites mal dormidas desregulam hormônios ligados à fome e ao estresse, dificultando a queima de gordura e a disposição para treinar.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 21,
                column: "texto",
                value: "Evite compensar o gasto do treino com excessos alimentares: a consistência diária entre exercício e nutrição é o que constrói resultados duradouros na composição corporal.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 22,
                column: "texto",
                value: "Acompanhe sua evolução por medidas corporais, roupas e desempenho nos treinos, pois a balança isoladamente não diferencia perda de gordura de ganho de massa muscular.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 23,
                column: "texto",
                value: "Execute os exercícios multiarticulares com empenho e boa amplitude, pois movimentos que recrutam grandes grupos musculares demandam muito mais energia do organismo.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 24,
                column: "texto",
                value: "Mesmo nos dias de menor disposição, cumpra a sessão planejada: manter o hábito ativo é mais eficiente para o emagrecimento do que buscar perfeição apenas em alguns dias.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 25,
                column: "texto",
                value: "Respeite integralmente os tempos mais longos de descanso entre as séries: eles são indispensáveis para restaurar os estoques de ATP-CP e o sistema nervoso central.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 26,
                column: "texto",
                value: "Ative firmemente o core e controle a respiração (estabilização intra-abdominal) antes de cada repetição pesada para proteger a coluna e transferir mais força ao movimento.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 27,
                column: "texto",
                value: "Realize séries progressivas de aquecimento específico no primeiro exercício do dia antes de entrar na carga máxima de trabalho, preparando articulações e recrutamento neural.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 28,
                column: "texto",
                value: "Imprima intenção de velocidade na fase concêntrica (subida da carga) mantendo controle total na descida, maximizando o recrutamento das fibras musculares de maior potência.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 29,
                column: "texto",
                value: "No treino de força, a qualidade técnica vem sempre antes do ego: interrompa a série caso perceba perda do alinhamento articular ou compensações perigosas na postura.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 30,
                column: "texto",
                value: "Garanta pés bem plantados no solo, escápulas estabilizadas e pegada firme na barra ou equipamento antes de iniciar cada repetição para evitar dissipação de força.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 31,
                column: "texto",
                value: "Pequenos incrementos de carga ao longo das semanas geram grandes ganhos de força a médio prazo sem sobrecarregar tendões, ligamentos e articulações.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 32,
                column: "texto",
                value: "Priorize os exercícios multiarticulares livres ou guiados do início da ficha enquanto o sistema nervoso está descansado, pois são eles que constroem sua base de força.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 33,
                column: "texto",
                value: "Mantenha o cardio leve conforme prescrito na ficha apenas como suporte cardiovascular e aquecimento, preservando suas reservas energéticas para as séries pesadas.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 34,
                column: "texto",
                value: "Cuide da mobilidade de tornozelos, quadris e ombros regularmente: articulações móveis e estáveis permitem alavancas mecânicas muito mais seguras e eficientes sob carga.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 35,
                column: "texto",
                value: "Valorize os dias de descanso da sua divisão semanal, pois adaptações neurais e estruturais de força exigem recuperação completa entre sessões intensas.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 36,
                column: "texto",
                value: "Mantenha um bom aporte energético e hidratação adequada antes de treinar; estoques plenos de glicogênio muscular são fundamentais para sustentar cargas elevadas.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 37,
                column: "texto",
                value: "Mantenha um ritmo cadenciado e respiração fluida do início ao fim de cada série longa, evitando prender o ar para retardar a instalação precoce da fadiga periférica.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 38,
                column: "texto",
                value: "Controle a escolha da carga para completar toda a faixa de repetições altas prescrita mantendo a postura impecável até a última repetição da série.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 39,
                column: "texto",
                value: "Respeite os intervalos reduzidos de descanso entre as séries para estimular a capilarização muscular, a eficiência mitocondrial e a tolerância à acidose metabólica.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 40,
                column: "texto",
                value: "Capriche na hidratação ao longo do dia e durante a sessão: a perda hídrica reduz rapidamente o rendimento em treinos de resistência muscular e aeróbica.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 41,
                column: "texto",
                value: "Execute o cardio contínuo pós-treino na zona aeróbica orientada para fortalecer o sistema cardiorrespiratório e acelerar a remoção de metabólitos entre os esforços.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 42,
                column: "texto",
                value: "Não encurte a amplitude das repetições quando a queimação muscular surgir: manter o movimento completo sob fadiga controlada é o que desenvolve resistência real.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 43,
                column: "texto",
                value: "Garanta uma ingestão equilibrada de carboidratos de qualidade na rotina alimentar para manter os estoques de energia necessários às sessões de maior volume de repetições.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 44,
                column: "texto",
                value: "Mantenha ombros, pescoço e musculaturas acessórias livres de tensão desnecessária durante o exercício para economizar energia e focar o esforço nos músculos-alvo.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 45,
                column: "texto",
                value: "A evolução na resistência aparece quando o mesmo esforço passa a parecer mais leve e sua recuperação entre as séries se torna cada vez mais rápida.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 46,
                column: "texto",
                value: "Alongamentos leves e exercícios de mobilidade após a sessão ajudam a aliviar a rigidez muscular e preservar a fluidez articular ao longo da semana de treinos.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 47,
                column: "texto",
                value: "Mantenha regularidade absoluta na frequência semanal: adaptações aeróbicas e de resistência muscular respondem de forma excelente à continuidade dos estímulos.");

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 48,
                column: "texto",
                value: "Preste atenção redobrada à estabilidade do core no final das séries longas, garantindo que a fadiga não comprometa o alinhamento da coluna lombar.");
        }
    }
}
