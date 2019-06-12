import React from "react";
import Header from "./common/Header";
import PageNotFound from "./PageNotFound";
import HomePage from "./home/HomePage";
import LoginPage from "./auth/LoginPage";
import AdminPage from "./admin/AdminPage";
import CashierPage from "./cashier/CashierPage";
import BaristaPage from "./barista/BaristaPage";
import ManagerPage from "./manager/ManagerPage";
import WaiterPage from "./waiter/WaiterPage";
import ManageUserPage from "./admin/ManageUserPage";
import UserProvider from "./UserProvider";
import ManageTabPage from "./waiter/ManageTabPage";
import Logout from "./auth/Logout";
import { Route, Switch } from "react-router-dom";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./font-awesome.css";

const App = () => {
  return (
    <div className="container-fluid">
      <Header />
      <Switch>
        <Route exact path="/" component={HomePage} />
        <Route path="/login" component={LoginPage} />
        <Route path="/logout" component={Logout} />
        <Route path="/admin" component={AdminPage} />
        <Route path="/manageUser/:userId" component={ManageUserPage} />
        <Route path="/cashier" component={CashierPage} />
        <Route path="/barista" component={BaristaPage} />
        <Route path="/manager" component={ManagerPage} />
        <Route path="/waiter" component={WaiterPage} />
        <Route path="/tab/:tabId" component={ManageTabPage} />
        <Route component={PageNotFound} />
      </Switch>
      <UserProvider />
      <ToastContainer autoClose={3000} hideProgressBar={true} />
    </div>
  );
};

export default App;
