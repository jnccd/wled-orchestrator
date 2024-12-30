import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";

const WledOrchThemeEditor = () => {
  // React Query setup
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  return <div>WledOrchThemeEditor</div>;
};

export default WledOrchThemeEditor;
