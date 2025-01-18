import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { HStack } from "@chakra-ui/react";
import WledSegmentGroupViewer from "./WledSegmentGroupViewer";
import { useQuery } from "@tanstack/react-query";

interface Props {}

const WledSegmentGroupsViewer = ({}: Props) => {
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  return (
    !wledOrchStateQuery.isPending && (
      <HStack justifyContent="center" width="100%" flexWrap="wrap">
        {wledOrchStateQuery.data?.groups?.map((g) => (
          <WledSegmentGroupViewer group={g} key={g.id}></WledSegmentGroupViewer>
        ))}
      </HStack>
    )
  );
};

export default WledSegmentGroupsViewer;
