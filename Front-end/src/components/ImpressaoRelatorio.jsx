import { GlobalStyles } from "@mui/material";

export default function ImpressaoRelatorio() {
  return (
    <GlobalStyles
      styles={{
        "@page": { size: "A4", margin: "15mm" },
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
          ".relatorio-impressao": { color: "#000 !important" },
          ".relatorio-impressao .MuiPaper-root": {
            background: "#fff !important",
            backgroundImage: "none !important",
            color: "#000 !important",
            boxShadow: "none !important",
            backdropFilter: "none !important",
            border: "1px solid #ddd !important",
          },
          ".relatorio-impressao .MuiTypography-root, .relatorio-impressao .MuiTableCell-root":
            {
              color: "#000 !important",
            },
          ".relatorio-impressao .MuiChip-root": {
            color: "#000 !important",
            borderColor: "#aaa !important",
          },
          ".relatorio-impressao .MuiTableContainer-root": {
            overflow: "visible !important",
          },
          ".relatorio-impressao thead": { display: "table-header-group" },
          ".relatorio-impressao tr": { breakInside: "avoid" },
        },
      }}
    />
  );
}
