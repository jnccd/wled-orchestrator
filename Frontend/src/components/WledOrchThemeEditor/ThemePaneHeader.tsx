import { Heading } from "@chakra-ui/react";
import { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

const ThemePaneHeader = ({ children }: Props) => {
  return (
    <Heading fontSize={"1.5rem"} padding={4} paddingBottom={6}>
      {children}
    </Heading>
  );
};

export default ThemePaneHeader;
