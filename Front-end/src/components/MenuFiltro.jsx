import { useState } from "react";
import { Button, Chip, Menu } from "@mui/material";
import FilterListIcon from "@mui/icons-material/FilterList";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";

export function ChipFiltro(props) {
  return (
    <Chip
      size="small"
      variant="outlined"
      sx={{ ml: 1.5, height: 20, fontSize: "0.7rem", fontWeight: 600 }}
      {...props}
    />
  );
}

export default function MenuFiltro({
  texto,
  ativo,
  ariaLabel,
  minWidth = 260,
  maxHeight,
  children,
}) {
  const [anchorEl, setAnchorEl] = useState(null);
  const aberto = Boolean(anchorEl);

  return (
    <>
      <Button
        variant="outlined"
        color={ativo ? "primary" : "inherit"}
        startIcon={<FilterListIcon />}
        endIcon={<KeyboardArrowDownIcon />}
        onClick={(e) => setAnchorEl(e.currentTarget)}
        aria-haspopup="menu"
        aria-expanded={aberto ? "true" : undefined}
        aria-label={ariaLabel}
        sx={{
          minHeight: 40,
          px: 2,
          borderColor: ativo ? "primary.main" : "divider",
          fontWeight: 500,
          whiteSpace: "nowrap",
        }}
      >
        {texto}
      </Button>
      <Menu
        anchorEl={anchorEl}
        open={aberto}
        onClose={() => setAnchorEl(null)}
        slotProps={{
          paper: { sx: { minWidth, maxHeight, borderRadius: 2, mt: 0.5 } },
        }}
      >
        {children}
      </Menu>
    </>
  );
}
