import { Box, HStack, Text, useColorMode } from "@chakra-ui/react";
import WledSegmentViewer from "./WledSegmentViewer";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import { LedSegmentGroup } from "../../hooks/useWledOrchApi";
import EditGroupButton from "./EditGroupButton";

const ledSegmentClassName = "led-segment-group";

interface Props {
  group: LedSegmentGroup;
}

const WledSegmentGroupViewer = ({ group: g }: Props) => {
  const { colorMode } = useColorMode();
  const selectedGroupStore = useSelectedGroupStore();

  return (
    <Box
      key={g.id}
      className={ledSegmentClassName + " " + (g.id ?? "error-id-less-group")}
      outline={`solid ${
        selectedGroupStore.selectedGroup === g.id ? "3px" : "1px"
      } ${colorMode === "dark" ? "#3F444E" : "#E2E8F0"}`}
      borderRadius="lg"
      margin="5px"
      padding="2px"
      width={"fit-content"}
      transition="outline .1s"
      onClick={(e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
        const elemsAtPoint = document.elementsFromPoint(e.clientX, e.clientY);
        const filteredElems = elemsAtPoint.filter((x) =>
          x.classList.contains("consumes-click")
        );
        if (filteredElems.length > 0) return;

        if (g.id) selectedGroupStore.selectNew(g.id);
      }}
    >
      <HStack justifyContent={"center"} gap={0} padding={1} flexWrap={"wrap"}>
        <Text
          fontWeight={
            selectedGroupStore.selectedGroup === g.id ? "bold" : "normal"
          }
          margin={2}
        >
          {g.name}
        </Text>
        <EditGroupButton group={g}></EditGroupButton>
      </HStack>
      <HStack gap={0}>
        {g.ledSegments &&
          g.ledSegments.map((s) => (
            <WledSegmentViewer
              ledSegmentClassName={ledSegmentClassName}
              segment={s}
              key={s.id}
            ></WledSegmentViewer>
          ))}
      </HStack>
    </Box>
  );
};

export default WledSegmentGroupViewer;
