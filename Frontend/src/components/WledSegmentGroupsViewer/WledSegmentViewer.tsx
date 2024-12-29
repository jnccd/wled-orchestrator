import { HStack, Text } from "@chakra-ui/react";
import {
  moveSegment,
  wledOrchStateQueryKey,
  getWledOrchState,
} from "../../hooks/useWledOrchState";
import { components } from "../../types/api";
import Draggable from "../Draggable";
import EditNameButton from "../EditNameButton";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

const serverButtonIdPrefix = "server-button";

interface Props {
  ledSegmentClassName: string;
  segment: components["schemas"]["LedSegment"];
}

const WledSegmentViewer = ({ segment: s, ledSegmentClassName }: Props) => {
  // React Query setup
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const mutation = useMutation({
    mutationFn: moveSegment,
    onSuccess: () => {
      // Invalidate and refetch
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const onDragEnd = (elem: HTMLElement, mousePos: number[]) => {
    const hitElements = document.elementsFromPoint(mousePos[0], mousePos[1]);
    const hitGroup = hitElements.filter((x) =>
      x.classList.contains(ledSegmentClassName)
    );
    const hitGroupData = query.data?.groups?.filter(
      (x) => x.id === hitGroup[0]?.classList[1]
    );

    mutation.mutate({
      segmentId: elem.classList[0],
      targetGroupId: hitGroupData ? hitGroupData[0]?.id ?? null : null,
    });
  };

  const lastAddressByte = s.wledServerAddress?.split(".").slice(-1)[0];

  return (
    <Draggable
      key={s.readonlyId}
      className={s.readonlyId ?? "error-id-less-segment"}
      id={serverButtonIdPrefix + "-" + lastAddressByte}
      onDragEnd={onDragEnd}
    >
      <HStack>
        <Text>{lastAddressByte ?? "Not Found"}</Text>
        <EditNameButton defaultValue={lastAddressByte}></EditNameButton>
      </HStack>
    </Draggable>
  );
};

export default WledSegmentViewer;
