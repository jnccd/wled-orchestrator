import "./App.css";
import NavBar from "./components/NavBar";
import { Grid, GridItem, Divider } from "@chakra-ui/react";
import WledSegmentGroupsViewer from "./components/WledSegmentGroupsViewer/WledSegmentGroupsViewer";
import WledOrchThemeEditor from "./components/WledOrchThemeEditor/WledOrchThemeEditor";
import {
  useMaxPageWidthStore,
  useSelectedGroupStore,
} from "./hooks/useLocalStore";

function App() {
  const selectedGroupStore = useSelectedGroupStore();
  selectedGroupStore.initialize();

  const maxPageWidthStore = useMaxPageWidthStore();

  return (
    <>
      <Grid
        templateAreas={{
          base: `"nav" "main"`,
        }}
        templateColumns={{
          base: "1fr",
        }}
        width={"100%"}
        height={"100%"}
      >
        <GridItem area="nav">
          <NavBar></NavBar>
        </GridItem>
        <GridItem
          area="main"
          margin={"0 auto"}
          maxWidth={maxPageWidthStore.maxPageWidth}
        >
          <br />
          <WledSegmentGroupsViewer></WledSegmentGroupsViewer>
          <br />
          <Divider width="90%" margin="0rem auto" opacity={1} />
          <br />
          <WledOrchThemeEditor></WledOrchThemeEditor>
        </GridItem>
      </Grid>
    </>
  );
}

export default App;
