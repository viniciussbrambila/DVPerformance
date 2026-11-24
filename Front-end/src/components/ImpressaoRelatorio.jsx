import { useEffect, useMemo, useState } from "react";
import { flushSync } from "react-dom";
import { GlobalStyles } from "@mui/material";
import { formatarDataHoraEmissao } from "../utils/relatorios";

const ESTILOS_BASE_IMPRESSAO = {
  "@page": {
    size: "A4",
    // A margem superior zerada oculta os dados automáticos do navegador.
    // A inferior reserva espaço real para a contagem das páginas.
    margin: "0 15mm 15mm",
    "@bottom-right": {
      content: 'counter(page) "/" counter(pages)',
      fontFamily: "Arial, sans-serif",
      fontSize: "10pt",
      fontWeight: 400,
      color: "#000",
      textAlign: "right",
      verticalAlign: "middle",
    },
  },
  "@media print": {
    ".MuiDrawer-root, .no-print": { display: "none !important" },
    "html, body": {
      background: "#fff !important",
      color: "#000 !important",
    },
    main: {
      padding: "0 !important",
      overflow: "visible !important",
      minHeight: "auto !important",
    },
    ".relatorio-impressao, .plano-impressao": {
      color: "#000 !important",
      paddingBottom: 0,
      boxDecorationBreak: "clone",
      WebkitBoxDecorationBreak: "clone",
    },
    ".relatorio-impressao .MuiPaper-root, .plano-impressao .MuiPaper-root": {
      background: "#fff !important",
      backgroundImage: "none !important",
      color: "#000 !important",
      boxShadow: "none !important",
      backdropFilter: "none !important",
      border: "1px solid #ddd !important",
    },
    ".plano-impressao .MuiPaper-root": {
      border: "none !important",
      padding: "0 !important",
      borderRadius: 0,
      boxDecorationBreak: "clone",
      WebkitBoxDecorationBreak: "clone",
    },
    ".relatorio-impressao .MuiTypography-root, .relatorio-impressao .MuiTableCell-root, .plano-impressao .MuiTypography-root, .plano-impressao .MuiTableCell-root":
      {
        color: "#000 !important",
      },
    ".relatorio-impressao .MuiChip-root": {
      color: "#000 !important",
      borderColor: "#aaa !important",
    },
    ".relatorio-impressao .MuiTableContainer-root, .plano-impressao .MuiTableContainer-root":
      {
        overflow: "visible !important",
      },
    ".relatorio-impressao thead, .plano-impressao thead": {
      display: "table-header-group",
    },
    ".relatorio-impressao tr, .plano-impressao tr": {
      breakInside: "avoid",
    },
    ".plano-impressao .info-faixa": {
      backgroundColor: "#f3f4f6 !important",
      printColorAdjust: "exact",
      WebkitPrintColorAdjust: "exact",
    },
    ".plano-impressao .rotulo-dia": {
      backgroundColor: "#000 !important",
      color: "#fff !important",
      printColorAdjust: "exact",
      WebkitPrintColorAdjust: "exact",
    },
  },
};

export default function ImpressaoRelatorio({
  margemSuperior = "15mm",
  mostrarEmissao = false,
}) {
  const [dataEmissao, setDataEmissao] = useState(() => new Date());

  useEffect(() => {
    if (!mostrarEmissao) return;

    const atualizarEmissao = () => {
      flushSync(() => {
        setDataEmissao(new Date());
      });
    };

    window.addEventListener("beforeprint", atualizarEmissao);
    return () => window.removeEventListener("beforeprint", atualizarEmissao);
  }, [mostrarEmissao]);

  const estilos = useMemo(
    () => ({
      ...ESTILOS_BASE_IMPRESSAO,
      "@page": {
        ...ESTILOS_BASE_IMPRESSAO["@page"],
        ...(mostrarEmissao
          ? {
              "@bottom-left": {
                content: JSON.stringify(
                  `Emitido em: ${formatarDataHoraEmissao(dataEmissao)}`,
                ),
                fontFamily: "Arial, sans-serif",
                fontSize: "10pt",
                fontWeight: 400,
                color: "#000",
                textAlign: "left",
                verticalAlign: "middle",
              },
            }
          : {}),
      },
      "@media print": {
        ...ESTILOS_BASE_IMPRESSAO["@media print"],
        ".relatorio-impressao, .plano-impressao": {
          ...ESTILOS_BASE_IMPRESSAO["@media print"][
            ".relatorio-impressao, .plano-impressao"
          ],
          paddingTop: margemSuperior,
        },
      },
    }),
    [margemSuperior, mostrarEmissao, dataEmissao],
  );

  return <GlobalStyles styles={estilos} />;
}
