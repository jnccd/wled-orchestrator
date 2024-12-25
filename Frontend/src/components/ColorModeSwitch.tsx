import { useColorMode } from "@chakra-ui/react";
import { IoMoon } from "react-icons/io5";
import { IoMoonOutline } from "react-icons/io5";

const ColorModeSwitch = () => {
  const { toggleColorMode, colorMode } = useColorMode();

  return (
    <button
      style={{ border: "none", background: "none", outline: "none" }}
      onClick={toggleColorMode}
    >
      {colorMode !== "dark" && <IoMoonOutline size={25} />}
      {colorMode === "dark" && <IoMoon size={25} />}
    </button>
  );
};

export default ColorModeSwitch;
